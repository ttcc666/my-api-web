using MyApiWeb.Models.Entities.System;

namespace MyApiWeb.Infrastructure.Data.SeedConfigurations
{
    public static class MenuSeedConfig
    {
        private static readonly string HomeMenuId = Guid.NewGuid().ToString();
        private static readonly string SystemManagementMenuId = Guid.NewGuid().ToString();
        private static readonly string UserManagementMenuId = Guid.NewGuid().ToString();
        private static readonly string RoleManagementMenuId = Guid.NewGuid().ToString();
        private static readonly string MenuManagementMenuId = Guid.NewGuid().ToString();
        private static readonly string SystemMonitorMenuId = Guid.NewGuid().ToString();
        private static readonly string OnlineUserManagementMenuId = Guid.NewGuid().ToString();

        public static List<Menu> GetMenus() => new()
        {
            // 主页 - 位于 frontend/src/views/dashboard/Home.vue
            new() { Id = HomeMenuId, Code = "home", Title = "主页", RoutePath = "/", RouteName = "home", Icon = "HomeOutline", Order = 0, Type = MenuType.Route, IsEnabled = true },

            // 系统管理目录
            new() { Id = SystemManagementMenuId, Code = "system-management", Title = "系统管理", Icon = "SettingsOutline", Order = 100, Type = MenuType.Directory, IsEnabled = true },

            // 用户管理 - 位于 frontend/src/views/system/UserManagement.vue
            new() { Id = UserManagementMenuId, Code = "user-management", Title = "用户管理", RoutePath = "/system/users", RouteName = "UserManagement", Icon = "PersonOutline", ParentId = SystemManagementMenuId, Order = 110, Type = MenuType.Route, PermissionCode = "system:user:view", IsEnabled = true },

            // 角色管理 - 位于 frontend/src/views/system/RoleManagement.vue
            new() { Id = RoleManagementMenuId, Code = "role-management", Title = "角色管理", RoutePath = "/system/roles", RouteName = "RoleManagement", Icon = "ShieldCheckmarkOutline", ParentId = SystemManagementMenuId, Order = 120, Type = MenuType.Route, PermissionCode = "system:role:view", IsEnabled = true },

            // 菜单管理 - 位于 frontend/src/views/system/MenuManagement.vue
            new() { Id = MenuManagementMenuId, Code = "menu-management", Title = "菜单管理", RoutePath = "/system/menus", RouteName = "MenuManagement", Icon = "MenuOutline", ParentId = SystemManagementMenuId, Order = 130, Type = MenuType.Route, PermissionCode = "system:menu:view", IsEnabled = true },

            // 在线用户管理 - 位于 frontend/src/views/hub/OnlineUserManagement.vue
            new()
                {
                    Id = OnlineUserManagementMenuId,
                    Code = "online-user-management",
                    Title = "在线用户管理",
                    RoutePath = "/hub/online-users",
                    RouteName = "OnlineUserManagement",
                    Icon = "TeamOutlined",
                    ParentId = SystemManagementMenuId,
                    Order = 140,
                    Type = MenuType.Route,
                    PermissionCode = "system:onlineuser:view",
                    IsEnabled = true
                },

            // 系统监控 - 位于 frontend/src/views/dashboard/SystemMonitor.vue
            new() { Id = SystemMonitorMenuId, Code = "system-monitor", Title = "系统监控", RoutePath = "/dashboard/system-monitor", RouteName = "SystemMonitor", Icon = "DesktopOutline", ParentId = SystemManagementMenuId, Order = 150, Type = MenuType.Route, PermissionCode = "system:monitor:view", IsEnabled = true }
        };
    }
}
