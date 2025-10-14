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
        public async Task SeedAsync()
        {
            try
            {
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
        /// 初始化权限数据（批量插入）
        /// </summary>
        private async Task SeedPermissionsAsync()
        {
            // 检查是否已有权限数据
            var existingCount = await _dbContext.Queryable<Permission>().CountAsync();
            if (existingCount > 0)
            {
                _logger.LogInformation("权限数据已存在，跳过初始化（共 {Count} 条）", existingCount);
                return;
            }

            var permissions = new List<Permission>
            {
                // 用户管理权限
                new Permission { Id = Guid.NewGuid().ToString(), Name = "user:view", DisplayName = "查看用户", Description = "查看用户列表和详情", Group = "用户管理" },
                new Permission { Id = Guid.NewGuid().ToString(), Name = "user:create", DisplayName = "创建用户", Description = "创建新用户", Group = "用户管理" },
                new Permission { Id = Guid.NewGuid().ToString(), Name = "user:edit", DisplayName = "编辑用户", Description = "编辑用户信息", Group = "用户管理" },
                new Permission { Id = Guid.NewGuid().ToString(), Name = "user:delete", DisplayName = "删除用户", Description = "删除用户", Group = "用户管理" },
                new Permission { Id = Guid.NewGuid().ToString(), Name = "user:assign-role", DisplayName = "分配角色", Description = "为用户分配角色", Group = "用户管理" },
                new Permission { Id = Guid.NewGuid().ToString(), Name = "user:assign-permission", DisplayName = "分配权限", Description = "为用户直接分配权限", Group = "用户管理" },

                // 角色管理权限
                new Permission { Id = Guid.NewGuid().ToString(), Name = "role:view", DisplayName = "查看角色", Description = "查看角色列表和详情", Group = "角色管理" },
                new Permission { Id = Guid.NewGuid().ToString(), Name = "role:create", DisplayName = "创建角色", Description = "创建新角色", Group = "角色管理" },
                new Permission { Id = Guid.NewGuid().ToString(), Name = "role:edit", DisplayName = "编辑角色", Description = "编辑角色信息", Group = "角色管理" },
                new Permission { Id = Guid.NewGuid().ToString(), Name = "role:delete", DisplayName = "删除角色", Description = "删除角色", Group = "角色管理" },
                new Permission { Id = Guid.NewGuid().ToString(), Name = "role:assign-permission", DisplayName = "分配权限", Description = "为角色分配权限", Group = "角色管理" },

                // 权限管理权限
                new Permission { Id = Guid.NewGuid().ToString(), Name = "permission:view", DisplayName = "查看权限", Description = "查看权限列表和详情", Group = "权限管理" },
                new Permission { Id = Guid.NewGuid().ToString(), Name = "permission:create", DisplayName = "创建权限", Description = "创建新权限", Group = "权限管理" },
                new Permission { Id = Guid.NewGuid().ToString(), Name = "permission:edit", DisplayName = "编辑权限", Description = "编辑权限信息", Group = "权限管理" },
                new Permission { Id = Guid.NewGuid().ToString(), Name = "permission:delete", DisplayName = "删除权限", Description = "删除权限", Group = "权限管理" },

                // 系统管理权限
                new Permission { Id = Guid.NewGuid().ToString(), Name = "system:settings", DisplayName = "系统设置", Description = "访问系统设置", Group = "系统管理" },
                new Permission { Id = Guid.NewGuid().ToString(), Name = "system:logs", DisplayName = "系统日志", Description = "查看系统日志", Group = "系统管理" },
                new Permission { Id = Guid.NewGuid().ToString(), Name = "system:backup", DisplayName = "数据备份", Description = "执行数据备份", Group = "系统管理" },

                // 仪表板权限
                new Permission { Id = Guid.NewGuid().ToString(), Name = "dashboard:view", DisplayName = "查看仪表板", Description = "访问系统仪表板", Group = "仪表板" },
                new Permission { Id = Guid.NewGuid().ToString(), Name = "dashboard:statistics", DisplayName = "查看统计", Description = "查看系统统计信息", Group = "仪表板" }
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
                _logger.LogInformation("角色数据已存在，跳过初始化（共 {Count} 条）", existingCount);
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
