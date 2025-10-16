using MyApiWeb.Models.Entities;

namespace MyApiWeb.Infrastructure.Data.SeedConfigurations
{
    public static class MenuSeedConfig
    {
        public const string HomeMenuId = "2f9f4d31-84f3-4a29-8b92-6d1b2c02fa1d";
        public const string SystemManagementMenuId = "6c1bdd65-0f24-4eb3-9b3f-4f0c1ecfea28";
        public const string UserManagementMenuId = "58cdb3a0-0c6a-4c18-9b0c-a739f5db6d8d";
        public const string RoleManagementMenuId = "f6466fee-a4ff-4333-9d25-bbf83e77dae1";
        public const string MenuManagementMenuId = "ab8dd1aa-139c-4c0a-9e9d-5ec52ffbdf98";
        public const string SystemMonitorMenuId = "c8e9f2b1-5d3a-4f7c-9e1b-2a6d8c4f0e3a";

        public static List<Menu> GetMenus() => new()
        {
            new() { Id = HomeMenuId, Code = "home", Title = "主页", RoutePath = "/", RouteName = "home", Icon = "HomeOutline", Order = 0, Type = MenuType.Route, IsEnabled = true },
            new() { Id = SystemManagementMenuId, Code = "system-management", Title = "系统管理", Icon = "SettingsOutline", Order = 100, Type = MenuType.Directory, IsEnabled = true },
            new() { Id = UserManagementMenuId, Code = "user-management", Title = "用户管理", RoutePath = "/admin/users", RouteName = "user-management", Icon = "PersonOutline", ParentId = SystemManagementMenuId, Order = 110, Type = MenuType.Route, PermissionCode = "system:user:view", IsEnabled = true },
            new() { Id = RoleManagementMenuId, Code = "role-management", Title = "角色管理", RoutePath = "/admin/roles", RouteName = "role-management", Icon = "ShieldCheckmarkOutline", ParentId = SystemManagementMenuId, Order = 120, Type = MenuType.Route, PermissionCode = "system:role:view", IsEnabled = true },
            new() { Id = MenuManagementMenuId, Code = "menu-management", Title = "菜单管理", RoutePath = "/admin/menus", RouteName = "menu-management", Icon = "MenuOutline", ParentId = SystemManagementMenuId, Order = 130, Type = MenuType.Route, PermissionCode = "system:menu:view", IsEnabled = true },
            new() { Id = SystemMonitorMenuId, Code = "system-monitor", Title = "系统监控", RoutePath = "/admin/system-monitor", RouteName = "SystemMonitor", Icon = "DesktopOutline", ParentId = SystemManagementMenuId, Order = 140, Type = MenuType.Route, PermissionCode = "system:monitor:view", IsEnabled = true }
        };
    }
}