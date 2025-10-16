using MyApiWeb.Models.Entities;

namespace MyApiWeb.Infrastructure.Data.SeedConfigurations
{
    public static class PermissionSeedConfig
    {
        public static List<Permission> GetPermissions() => new()
        {
            new() { Id = Guid.NewGuid().ToString(), Name = "system:user:view", DisplayName = "查看用户", Description = "查看用户列表和详情", Group = "用户管理" },
            new() { Id = Guid.NewGuid().ToString(), Name = "system:role:view", DisplayName = "查看角色", Description = "查看角色列表和详情", Group = "角色管理" },
            new() { Id = Guid.NewGuid().ToString(), Name = "system:permission:view", DisplayName = "查看权限", Description = "查看权限列表和详情", Group = "权限管理" },
            new() { Id = Guid.NewGuid().ToString(), Name = "system:menu:view", DisplayName = "查看菜单", Description = "查看菜单列表和详情", Group = "菜单管理" },
            new() { Id = Guid.NewGuid().ToString(), Name = "system:monitor:view", DisplayName = "查看系统监控", Description = "查看系统监控信息", Group = "系统监控" },
            new Permission
            {
                Id = Guid.NewGuid().ToString(),
                Name = "system:onlineuser:view",
                DisplayName = "查看在线用户",
                Description = "查看在线用户列表和统计信息",
                Group = "系统管理",
                IsEnabled = true
            }
        };
    }
}