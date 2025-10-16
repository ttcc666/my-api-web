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
    /// 权限服务实现
    /// </summary>
    public class PermissionService : IPermissionService
    {
        private readonly SqlSugarDbContext _dbContext;
        private readonly ICurrentUser _currentUser;

        public PermissionService(SqlSugarDbContext dbContext, ICurrentUser currentUser)
        {
            _dbContext = dbContext;
            _currentUser = currentUser;
        }

        public async Task<List<PermissionDto>> GetAllPermissionsAsync()
        {
            var permissions = await _dbContext.Queryable<Permission>()
                .OrderBy(p => p.Group + p.Name)
                .ToListAsync();

            return permissions.Select(MapToPermissionDto).ToList();
        }

        public async Task<List<PermissionGroupDto>> GetPermissionsByGroupAsync()
        {
            var permissions = await GetAllPermissionsAsync();

            var groups = permissions
                .GroupBy(p => p.Group ?? "其他")
                .Select(g => new PermissionGroupDto
                {
                    Group = g.Key,
                    Permissions = g.ToList()
                })
                .OrderBy(g => g.Group)
                .ToList();

            return groups;
        }

        public async Task<PermissionDto?> GetPermissionByIdAsync(string id)
        {
            var permission = await _dbContext.Queryable<Permission>()
                .Where(p => p.Id == id)
                .FirstAsync();

            return permission == null ? null : MapToPermissionDto(permission);
        }

        public async Task<PermissionDto> CreatePermissionAsync(CreatePermissionDto createPermissionDto)
        {
            // 检查权限名称是否已存在
            if (await PermissionNameExistsAsync(createPermissionDto.Name))
            {
                throw new BusinessException($"权限名称 '{createPermissionDto.Name}' 已存在");
            }

            var permission = new Permission
            {
                Id = Guid.NewGuid().ToString(),
                Name = createPermissionDto.Name,
                DisplayName = createPermissionDto.DisplayName,
                Description = createPermissionDto.Description,
                Group = createPermissionDto.Group,
                IsEnabled = createPermissionDto.IsEnabled
            };

            await _dbContext.Insertable(permission).ExecuteCommandAsync();

            return MapToPermissionDto(permission);
        }

        public async Task<PermissionDto> UpdatePermissionAsync(string id, UpdatePermissionDto updatePermissionDto)
        {
            var permission = await _dbContext.Queryable<Permission>()
                .Where(p => p.Id == id)
                .FirstAsync();

            if (permission == null)
            {
                throw new BusinessException("权限不存在");
            }

            // 检查权限名称是否已存在（排除当前权限）
            if (await PermissionNameExistsAsync(updatePermissionDto.Name, id))
            {
                throw new BusinessException($"权限名称 '{updatePermissionDto.Name}' 已存在");
            }

            permission.Name = updatePermissionDto.Name;
            permission.DisplayName = updatePermissionDto.DisplayName;
            permission.Description = updatePermissionDto.Description;
            permission.Group = updatePermissionDto.Group;
            permission.IsEnabled = updatePermissionDto.IsEnabled;

            await _dbContext.Updateable(permission).ExecuteCommandAsync();

            return MapToPermissionDto(permission);
        }

        public async Task<bool> DeletePermissionAsync(string id)
        {
            var permission = await _dbContext.Queryable<Permission>()
                .Where(p => p.Id == id)
                .FirstAsync();

            if (permission == null)
            {
                throw new BusinessException("权限不存在");
            }

            // 检查是否有角色使用此权限
            var hasRoles = await _dbContext.Queryable<RolePermission>()
                .Where(rp => rp.PermissionId == id)
                .AnyAsync();

            if (hasRoles)
            {
                throw new BusinessException("该权限正在被角色使用，无法删除");
            }

            // 检查是否有用户直接使用此权限
            var hasUsers = await _dbContext.Queryable<UserPermission>()
                .Where(up => up.PermissionId == id)
                .AnyAsync();

            if (hasUsers)
            {
                throw new BusinessException("该权限正在被用户使用，无法删除");
            }

            // 删除权限
            var result = await _dbContext.Deleteable<Permission>()
                .Where(p => p.Id == id)
                .ExecuteCommandAsync();

            return result > 0;
        }

        public async Task<bool> PermissionNameExistsAsync(string name, string? excludeId = null)
        {
            var query = _dbContext.Queryable<Permission>()
                .Where(p => p.Name == name);

            if (!string.IsNullOrEmpty(excludeId))
            {
                query = query.Where(p => p.Id != excludeId);
            }

            return await query.AnyAsync();
        }

        public async Task<UserPermissionInfoDto> GetUserPermissionsAsync(string userId)
        {
            // 获取用户信息
            var user = await _dbContext.Queryable<User>()
                .Where(u => u.Id == userId)
                .FirstAsync();

            if (user == null)
            {
                throw new BusinessException("用户不存在");
            }

            // 获取用户角色
            var userRoles = await _dbContext.Queryable<Role>()
                .InnerJoin<UserRole>((r, ur) => r.Id == ur.RoleId)
                .Where((r, ur) => ur.UserId == userId)
                .Select(r => r)
                .ToListAsync();

            // 获取用户直接权限
            var directPermissions = await _dbContext.Queryable<Permission>()
                .InnerJoin<UserPermission>((p, up) => p.Id == up.PermissionId)
                .Where((p, up) => up.UserId == userId)
                .Select(p => p)
                .ToListAsync();

            // 获取角色权限
            var rolePermissions = new List<Permission>();
            if (userRoles.Any())
            {
                var roleIds = userRoles.Select(r => r.Id).ToList();
                rolePermissions = await _dbContext.Queryable<Permission>()
                    .InnerJoin<RolePermission>((p, rp) => p.Id == rp.PermissionId)
                    .Where((p, rp) => roleIds.Contains(rp.RoleId))
                    .Select(p => p)
                    .ToListAsync();
            }

            // 合并权限（去重）
            var allPermissions = directPermissions.Concat(rolePermissions)
                .GroupBy(p => p.Id)
                .Select(g => g.First())
                .ToList();

            return new UserPermissionInfoDto
            {
                UserId = userId,
                Username = user.Username,
                Roles = userRoles.Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    IsSystem = r.IsSystem,
                    IsEnabled = r.IsEnabled,
                    CreationTime = r.CreationTime,
                    Permissions = new List<PermissionDto>()
                }).ToList(),
                DirectPermissions = directPermissions.Select(MapToPermissionDto).ToList(),
                EffectivePermissions = allPermissions.Select(MapToPermissionDto).ToList()
            };
        }

        public async Task<UserPermissionCheckDto> CheckUserPermissionAsync(string userId, string permission)
        {
            var userPermissions = await GetUserPermissionsAsync(userId);
            var hasPermission = userPermissions.EffectivePermissions.Any(p => p.Name == permission);

            var source = "无";
            if (hasPermission)
            {
                var isDirectPermission = userPermissions.DirectPermissions.Any(p => p.Name == permission);
                source = isDirectPermission ? "直接分配" : "角色继承";
            }

            return new UserPermissionCheckDto
            {
                UserId = userId,
                Permission = permission,
                HasPermission = hasPermission,
                Source = source
            };
        }

        public async Task<List<UserPermissionCheckDto>> CheckUserPermissionsAsync(string userId, CheckPermissionDto checkPermissionDto)
        {
            var results = new List<UserPermissionCheckDto>();

            foreach (var permission in checkPermissionDto.Permissions)
            {
                var result = await CheckUserPermissionAsync(userId, permission);
                results.Add(result);
            }

            return results;
        }

        public async Task<bool> AssignPermissionsToUserAsync(string userId, AssignUserPermissionsDto assignPermissionsDto)
        {
            // 验证用户是否存在
            var userExists = await _dbContext.Queryable<User>()
                .Where(u => u.Id == userId)
                .AnyAsync();

            if (!userExists)
            {
                throw new BusinessException("用户不存在");
            }

            // 验证所有权限是否存在
            var existingPermissions = await _dbContext.Queryable<Permission>()
                .Where(p => assignPermissionsDto.PermissionIds.Contains(p.Id))
                .ToListAsync();

            if (existingPermissions.Count != assignPermissionsDto.PermissionIds.Count)
            {
                throw new BusinessException("部分权限不存在");
            }

            // 删除用户现有的直接权限
            await _dbContext.Deleteable<UserPermission>()
                .Where(up => up.UserId == userId)
                .ExecuteCommandAsync();

            // 分配新权限
            var userPermissions = assignPermissionsDto.PermissionIds.Select(permissionId => new UserPermission
            {
                UserId = userId,
                PermissionId = permissionId,
                AssignedBy = _currentUser.Id?.ToString() ?? "system",
                AssignedTime = DateTimeOffset.Now
            }).ToList();

            await _dbContext.Insertable(userPermissions).ExecuteCommandAsync();

            return true;
        }

        public async Task<bool> RemovePermissionFromUserAsync(string userId, string permissionId)
        {
            var result = await _dbContext.Deleteable<UserPermission>()
                .Where(up => up.UserId == userId && up.PermissionId == permissionId)
                .ExecuteCommandAsync();

            return result > 0;
        }

        public async Task<List<PermissionDto>> GetUserDirectPermissionsAsync(string userId)
        {
            var permissions = await _dbContext.Queryable<Permission>()
                .InnerJoin<UserPermission>((p, up) => p.Id == up.PermissionId)
                .Where((p, up) => up.UserId == userId)
                .Select(p => p)
                .ToListAsync();

            return permissions.Select(MapToPermissionDto).ToList();
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