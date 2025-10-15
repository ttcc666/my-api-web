using Microsoft.Extensions.Logging;
using MyApiWeb.Models.Entities;
using MyApiWeb.Repository;
using SqlSugar;

namespace MyApiWeb.Infrastructure.Data
{
    /// <summary>
    /// 菜单数据种子初始化
    /// </summary>
    public class MenuDataSeeder
    {
        private readonly SqlSugarDbContext _dbContext;
        private readonly ILogger<MenuDataSeeder> _logger;

        private static readonly string HomeMenuId = "2f9f4d31-84f3-4a29-8b92-6d1b2c02fa1d";
        private static readonly string SystemManagementMenuId = "6c1bdd65-0f24-4eb3-9b3f-4f0c1ecfea28";
        private static readonly string UserManagementMenuId = "58cdb3a0-0c6a-4c18-9b0c-a739f5db6d8d";
        private static readonly string RoleManagementMenuId = "f6466fee-a4ff-4333-9d25-bbf83e77dae1";
        private static readonly string MenuManagementMenuId = "ab8dd1aa-139c-4c0a-9e9d-5ec52ffbdf98";

        public MenuDataSeeder(SqlSugarDbContext dbContext, ILogger<MenuDataSeeder> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// 初始化菜单数据
        /// </summary>
        public async Task SeedAsync()
        {
            try
            {
                await SeedMenusAsync();
                _logger.LogInformation("菜单数据种子初始化完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "菜单数据种子初始化失败");
                throw;
            }
        }

        private async Task SeedMenusAsync()
        {
            var menus = new List<Menu>
            {
                new Menu
                {
                    Id = HomeMenuId,
                    Code = "home",
                    Title = "主页",
                    RoutePath = "/",
                    RouteName = "home",
                    Icon = "HomeOutline",
                    Order = 0,
                    Type = MenuType.Route,
                    IsEnabled = true
                },
                new Menu
                {
                    Id = SystemManagementMenuId,
                    Code = "system-management",
                    Title = "系统管理",
                    Icon = "SettingsOutline",
                    Order = 100,
                    Type = MenuType.Directory,
                    IsEnabled = true
                },
                new Menu
                {
                    Id = UserManagementMenuId,
                    Code = "user-management",
                    Title = "用户管理",
                    RoutePath = "/admin/users",
                    RouteName = "user-management",
                    Icon = "PersonOutline",
                    ParentId = SystemManagementMenuId,
                    Order = 110,
                    Type = MenuType.Route,
                    PermissionCode = "user:view",
                    IsEnabled = true
                },
                new Menu
                {
                    Id = RoleManagementMenuId,
                    Code = "role-management",
                    Title = "角色管理",
                    RoutePath = "/admin/roles",
                    RouteName = "role-management",
                    Icon = "ShieldCheckmarkOutline",
                    ParentId = SystemManagementMenuId,
                    Order = 120,
                    Type = MenuType.Route,
                    PermissionCode = "role:view",
                    IsEnabled = true
                },
                new Menu
                {
                    Id = MenuManagementMenuId,
                    Code = "menu-management",
                    Title = "菜单管理",
                    RoutePath = "/admin/menus",
                    RouteName = "menu-management",
                    Icon = "MenuOutline",
                    ParentId = SystemManagementMenuId,
                    Order = 130,
                    Type = MenuType.Route,
                    PermissionCode = "menu:view",
                    IsEnabled = true
                }
            };

            foreach (var menu in menus)
            {
                var exists = await _dbContext.Queryable<Menu>()
                    .Where(m => m.Code == menu.Code)
                    .AnyAsync();

                if (!exists)
                {
                    await _dbContext.Insertable(menu).ExecuteCommandAsync();
                    _logger.LogInformation("创建菜单: {Code}", menu.Code);
                }
            }
        }
    }
}
