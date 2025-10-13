using MyApiWeb.Models.DTOs;
using MyApiWeb.Models.Entities;
using MyApiWeb.Models.Exceptions;
using MyApiWeb.Models.Interfaces;
using MyApiWeb.Repository;
using MyApiWeb.Services.Interfaces;
using SqlSugar;

namespace MyApiWeb.Services.Implements
{
    /// <summary>
    /// 角色服务实现
    /// </summary>
    public class RoleService : IRoleService
    {
        private readonly SqlSugarDbContext _dbContext;
        private readonly ICurrentUser _currentUser;

        public RoleService(SqlSugarDbContext dbContext, ICurrentUser currentUser)
        {
            _dbContext = dbContext;
            _currentUser = currentUser;
        }

        public async Task<List<RoleDto>> GetAllRolesAsync()
        {
            var roles = await _dbContext.Queryable<Role>()
                .OrderBy(r => r.CreationTime)
                .ToListAsync();

            var roleDtos = new List<RoleDto>();
            foreach (var role in roles)
            {
                var permissions = await GetRolePermissionsAsync(role.Id);
                roleDtos.Add(MapToRoleDto(role, permissions));
            }

            return roleDtos;
        }

        public async Task<RoleDto?> GetRoleByIdAsync(string id)
        {
            var role = await _dbContext.Queryable<Role>()
                .Where(r => r.Id == id)
                .FirstAsync();

            if (role == null)
                return null;

            var permissions = await GetRolePermissionsAsync(id);
            return MapToRoleDto(role, permissions);
        }

        public async Task<RoleDto> CreateRoleAsync(CreateRoleDto createRoleDto)
        {
            // 检查角色名称是否已存在
            if (await RoleNameExistsAsync(createRoleDto.Name))
            {
                throw new BusinessException($"角色名称 '{createRoleDto.Name}' 已存在");
            }

            var role = new Role
            {
                Id = Guid.NewGuid().ToString(),
                Name = createRoleDto.Name,
                Description = createRoleDto.Description,
                IsEnabled = createRoleDto.IsEnabled,
                IsSystem = false
            };

            await _dbContext.Insertable(role).ExecuteCommandAsync();

            // 分配权限
            if (createRoleDto.PermissionIds.Any())
            {
                await AssignPermissionsToRoleInternalAsync(role.Id, createRoleDto.PermissionIds);
            }

            var permissions = await GetRolePermissionsAsync(role.Id);
            return MapToRoleDto(role, permissions);
        }

        public async Task<RoleDto> UpdateRoleAsync(string id, UpdateRoleDto updateRoleDto)
        {
            var role = await _dbContext.Queryable<Role>()
                .Where(r => r.Id == id)
                .FirstAsync();

            if (role == null)
            {
                throw new BusinessException("角色不存在");
            }

            if (role.IsSystem)
            {
                throw new BusinessException("系统角色不允许修改");
            }

            // 检查角色名称是否已存在（排除当前角色）
            if (await RoleNameExistsAsync(updateRoleDto.Name, id))
            {
                throw new BusinessException($"角色名称 '{updateRoleDto.Name}' 已存在");
            }

            role.Name = updateRoleDto.Name;
            role.Description = updateRoleDto.Description;
            role.IsEnabled = updateRoleDto.IsEnabled;

            await _dbContext.Updateable(role).ExecuteCommandAsync();

            var permissions = await GetRolePermissionsAsync(id);
            return MapToRoleDto(role, permissions);
        }

        public async Task<bool> DeleteRoleAsync(string id)
        {
            var role = await _dbContext.Queryable<Role>()
                .Where(r => r.Id == id)
                .FirstAsync();

            if (role == null)
            {
                throw new BusinessException("角色不存在");
            }

            if (role.IsSystem)
            {
                throw new BusinessException("系统角色不允许删除");
            }

            // 检查是否有用户使用此角色
            var hasUsers = await _dbContext.Queryable<UserRole>()
                .Where(ur => ur.RoleId == id)
                .AnyAsync();

            if (hasUsers)
            {
                throw new BusinessException("该角色正在被用户使用，无法删除");
            }

            // 删除角色权限关联
            await _dbContext.Deleteable<RolePermission>()
                .Where(rp => rp.RoleId == id)
                .ExecuteCommandAsync();

            // 删除角色
            var result = await _dbContext.Deleteable<Role>()
                .Where(r => r.Id == id)
                .ExecuteCommandAsync();

            return result > 0;
        }

        public async Task<bool> AssignPermissionsToRoleAsync(string roleId, AssignRolePermissionsDto assignPermissionsDto)
        {
            var role = await _dbContext.Queryable<Role>()
                .Where(r => r.Id == roleId)
                .FirstAsync();

            if (role == null)
            {
                throw new BusinessException("角色不存在");
            }

            if (role.IsSystem)
            {
                throw new BusinessException("系统角色不允许修改权限");
            }

            return await AssignPermissionsToRoleInternalAsync(roleId, assignPermissionsDto.PermissionIds);
        }

        public async Task<List<PermissionDto>> GetRolePermissionsAsync(string roleId)
        {
            var permissions = await _dbContext.Queryable<Permission>()
                .InnerJoin<RolePermission>((p, rp) => p.Id == rp.PermissionId)
                .Where((p, rp) => rp.RoleId == roleId)
                .Select(p => p)
                .ToListAsync();

            return permissions.Select(MapToPermissionDto).ToList();
        }

        public async Task<bool> RoleNameExistsAsync(string name, string? excludeId = null)
        {
            var query = _dbContext.Queryable<Role>()
                .Where(r => r.Name == name);

            if (!string.IsNullOrEmpty(excludeId))
            {
                query = query.Where(r => r.Id != excludeId);
            }

            return await query.AnyAsync();
        }

        public async Task<List<RoleDto>> GetUserRolesAsync(string userId)
        {
            var roles = await _dbContext.Queryable<Role>()
                .InnerJoin<UserRole>((r, ur) => r.Id == ur.RoleId)
                .Where((r, ur) => ur.UserId == userId)
                .Select(r => r)
                .ToListAsync();

            var roleDtos = new List<RoleDto>();
            foreach (var role in roles)
            {
                var permissions = await GetRolePermissionsAsync(role.Id);
                roleDtos.Add(MapToRoleDto(role, permissions));
            }

            return roleDtos;
        }

        public async Task<bool> AssignRolesToUserAsync(string userId, AssignUserRolesDto assignRolesDto)
        {
            // 验证用户是否存在
            var userExists = await _dbContext.Queryable<User>()
                .Where(u => u.Id == userId)
                .AnyAsync();

            if (!userExists)
            {
                throw new BusinessException("用户不存在");
            }

            // 验证所有角色是否存在
            var existingRoles = await _dbContext.Queryable<Role>()
                .Where(r => assignRolesDto.RoleIds.Contains(r.Id))
                .ToListAsync();

            if (existingRoles.Count != assignRolesDto.RoleIds.Count)
            {
                throw new BusinessException("部分角色不存在");
            }

            // 删除用户现有的角色
            await _dbContext.Deleteable<UserRole>()
                .Where(ur => ur.UserId == userId)
                .ExecuteCommandAsync();

            // 分配新角色
            var userRoles = assignRolesDto.RoleIds.Select(roleId => new UserRole
            {
                UserId = userId,
                RoleId = roleId,
                AssignedBy = _currentUser.Id?.ToString() ?? "system",
                AssignedTime = DateTimeOffset.Now
            }).ToList();

            await _dbContext.Insertable(userRoles).ExecuteCommandAsync();

            return true;
        }

        public async Task<bool> RemoveRoleFromUserAsync(string userId, string roleId)
        {
            var result = await _dbContext.Deleteable<UserRole>()
                .Where(ur => ur.UserId == userId && ur.RoleId == roleId)
                .ExecuteCommandAsync();

            return result > 0;
        }

        private async Task<bool> AssignPermissionsToRoleInternalAsync(string roleId, List<string> permissionIds)
        {
            // 验证所有权限是否存在
            var existingPermissions = await _dbContext.Queryable<Permission>()
                .Where(p => permissionIds.Contains(p.Id))
                .ToListAsync();

            if (existingPermissions.Count != permissionIds.Count)
            {
                throw new BusinessException("部分权限不存在");
            }

            // 删除角色现有的权限
            await _dbContext.Deleteable<RolePermission>()
                .Where(rp => rp.RoleId == roleId)
                .ExecuteCommandAsync();

            // 分配新权限
            var rolePermissions = permissionIds.Select(permissionId => new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId,
                AssignedBy = _currentUser.Id?.ToString() ?? "system",
                AssignedTime = DateTimeOffset.Now
            }).ToList();

            await _dbContext.Insertable(rolePermissions).ExecuteCommandAsync();

            return true;
        }

        private static RoleDto MapToRoleDto(Role role, List<PermissionDto> permissions)
        {
            return new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                IsSystem = role.IsSystem,
                IsEnabled = role.IsEnabled,
                CreationTime = role.CreationTime,
                Permissions = permissions
            };
        }

        private static PermissionDto MapToPermissionDto(Permission permission)
        {
            return new PermissionDto
            {
                Id = permission.Id,
                Name = permission.Name,
                DisplayName = permission.DisplayName,
                Description = permission.Description,
                Group = permission.Group,
                IsEnabled = permission.IsEnabled,
                CreationTime = permission.CreationTime
            };
        }
    }
}