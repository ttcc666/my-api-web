using Microsoft.Extensions.Logging;
using MyApiWeb.Models.Entities;
using MyApiWeb.Repository;
using SqlSugar;

namespace MyApiWeb.Infrastructure.Data
{
    /// <summary>
    /// RBAC 权限管理数据种子类
    /// </summary>
    public class RbacDataSeeder
    {
        private readonly SqlSugarDbContext _dbContext;
        private readonly ILogger<RbacDataSeeder> _logger;

        // 固定的角色 ID（便于关联）
        private const string SuperAdminRoleId = "super-admin-role-id";
        private const string AdminRoleId = "admin-role-id";
        private const string UserRoleId = "user-role-id";

        public RbacDataSeeder(SqlSugarDbContext dbContext, ILogger<RbacDataSeeder> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// 初始化 RBAC 数据
        /// </summary>
        /// <param name="forceReinitialize">是否强制重新初始化（删除现有数据）</param>
        public async Task SeedAsync(bool forceReinitialize = false)
        {
            try
            {
                if (forceReinitialize)
                {
                    await ClearExistingDataAsync();
                    _logger.LogWarning("已清除所有 RBAC 数据，开始强制重新初始化");
                }

                await SeedPermissionsAsync();
                await SeedRolesAsync();
                await SeedSuperAdminAsync();

                _logger.LogInformation("RBAC 数据种子初始化完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RBAC 数据种子初始化失败");
                throw;
            }
        }

        /// <summary>
        /// 清除现有的 RBAC 数据
        /// </summary>
        private async Task ClearExistingDataAsync()
        {
            await _dbContext.Db.Deleteable<RolePermission>().ExecuteCommandAsync();
            await _dbContext.Db.Deleteable<UserRole>().ExecuteCommandAsync();
            await _dbContext.Db.Deleteable<Permission>().ExecuteCommandAsync();
            await _dbContext.Db.Deleteable<Role>().ExecuteCommandAsync();
            _logger.LogInformation("已清除所有角色、权限及关联数据");
        }

        /// <summary>
        /// 初始化权限数据（批量插入）
        /// </summary>
        private async Task SeedPermissionsAsync()
        {
            // 检查是否已有权限数据
            var existingCount = await _dbContext.Queryable<Permission>().CountAsync();
            if (existingCount > 0)
            {
                _logger.LogInformation("权限数据已存在，跳过强制初始化（共 {Count} 条）", existingCount);
                return;
            }

            var permissions = new List<Permission>
            {
                // 用户管理权限
                new Permission { Id = Guid.NewGuid().ToString(), Name = "system:user:view", DisplayName = "查看用户", Description = "查看用户列表和详情", Group = "用户管理" },

                // 角色管理权限
                new Permission { Id = Guid.NewGuid().ToString(), Name = "system:role:view", DisplayName = "查看角色", Description = "查看角色列表和详情", Group = "角色管理" },

                // 权限管理权限
                new Permission { Id = Guid.NewGuid().ToString(), Name = "system:permission:view", DisplayName = "查看权限", Description = "查看权限列表和详情", Group = "权限管理" },

                // 菜单管理权限
                new Permission { Id = Guid.NewGuid().ToString(), Name = "system:menu:view", DisplayName = "查看菜单", Description = "查看菜单列表和详情", Group = "菜单管理" },

                // 系统监控
                new Permission { Id = Guid.NewGuid().ToString(), Name = "system:monitor:view", DisplayName = "查看系统监控", Description = "查看系统监控信息", Group = "系统监控" }
            };

            // 批量插入权限
            await _dbContext.Db.Insertable(permissions).ExecuteCommandAsync();

            _logger.LogInformation("批量创建了 {Count} 个权限", permissions.Count);
        }

        /// <summary>
        /// 初始化角色数据（批量插入）
        /// </summary>
        private async Task SeedRolesAsync()
        {
            // 检查是否已有角色数据
            var existingCount = await _dbContext.Queryable<Role>().CountAsync();
            if (existingCount > 0)
            {
                _logger.LogInformation("角色数据已存在，跳过强制初始化（共 {Count} 条）", existingCount);
                // 仍然需要确保超级管理员有所有权限
                await AssignAllPermissionsToSuperAdminAsync();
                return;
            }

            var roles = new List<Role>
            {
                new Role
                {
                    Id = SuperAdminRoleId,
                    Name = "超级管理员",
                    Description = "系统超级管理员，拥有所有权限",
                    IsSystem = true,
                    IsEnabled = true
                },
                new Role
                {
                    Id = AdminRoleId,
                    Name = "管理员",
                    Description = "系统管理员，拥有大部分管理权限",
                    IsSystem = false,
                    IsEnabled = true
                },
                new Role
                {
                    Id = UserRoleId,
                    Name = "普通用户",
                    Description = "普通用户，拥有基础权限",
                    IsSystem = false,
                    IsEnabled = true
                }
            };

            // 批量插入角色
            await _dbContext.Db.Insertable(roles).ExecuteCommandAsync();

            _logger.LogInformation("批量创建了 {Count} 个角色", roles.Count);

            // 为超级管理员角色分配所有权限
            await AssignAllPermissionsToSuperAdminAsync();
        }

        /// <summary>
        /// 为超级管理员角色分配所有权限（批量插入）
        /// </summary>
        private async Task AssignAllPermissionsToSuperAdminAsync()
        {
            var allPermissions = await _dbContext.Queryable<Permission>().ToListAsync();

            // 获取已分配的权限
            var existingPermissionIds = await _dbContext.Queryable<RolePermission>()
                .Where(rp => rp.RoleId == SuperAdminRoleId)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            // 过滤出未分配的权限
            var newPermissions = allPermissions
                .Where(p => !existingPermissionIds.Contains(p.Id))
                .ToList();

            if (newPermissions.Count == 0)
            {
                _logger.LogInformation("超级管理员角色已拥有所有权限");
                return;
            }

            var systemUserId = "system";

            // 构建角色权限关联列表
            var rolePermissions = newPermissions.Select(permission => new RolePermission
            {
                RoleId = SuperAdminRoleId,
                PermissionId = permission.Id,
                AssignedBy = systemUserId,
                AssignedTime = DateTimeOffset.Now
            }).ToList();

            // 批量插入角色权限关联
            await _dbContext.Db.Insertable(rolePermissions).ExecuteCommandAsync();

            _logger.LogInformation("为超级管理员角色新增了 {Count} 个权限", newPermissions.Count);
        }

        /// <summary>
        /// 创建超级管理员用户
        /// </summary>
        private async Task SeedSuperAdminAsync()
        {
            var superAdminUsername = "admin";

            // 检查是否已存在超级管理员用户
            var existingAdmin = await _dbContext.Queryable<User>()
                .Where(u => u.Username == superAdminUsername)
                .FirstAsync();

            if (existingAdmin != null)
            {
                // 确保超级管理员拥有超级管理员角色
                var hasRole = await _dbContext.Queryable<UserRole>()
                    .Where(ur => ur.UserId == existingAdmin.Id && ur.RoleId == SuperAdminRoleId)
                    .AnyAsync();

                if (!hasRole)
                {
                    var userRole = new UserRole
                    {
                        UserId = existingAdmin.Id,
                        RoleId = SuperAdminRoleId,
                        AssignedBy = "system",
                        AssignedTime = DateTimeOffset.Now
                    };

                    await _dbContext.Insertable(userRole).ExecuteCommandAsync();
                    _logger.LogInformation("为现有管理员用户分配了超级管理员角色");
                }
            }
            else
            {
                _logger.LogWarning("未找到用户名为 '{Username}' 的管理员用户，请确保先执行管理员用户种子", superAdminUsername);
            }
        }
    }
}
