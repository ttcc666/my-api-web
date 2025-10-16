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
        /// <param name="forceReinitialize">是否强制重新初始化（删除现有数据）</param>
        public async Task SeedAsync(bool forceReinitialize = false)
        {
            try
            {
                if (forceReinitialize)
                {
                    await _dbContext.Db.Deleteable<Menu>().ExecuteCommandAsync();
                    _logger.LogWarning("已清除所有菜单数据，开始强制重新初始化");
                }

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
                    PermissionCode = "system:user:view",
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
                    PermissionCode = "system:role:view",
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
                    PermissionCode = "system:menu:view",
                    IsEnabled = true
                },
                new Menu
                {
                    Id = Guid.NewGuid().ToString(), // 请生成一个新的唯一ID
                    Code = "system-monitor",
                    Title = "系统监控",
                    RoutePath = "/admin/system-monitor",
                    RouteName = "SystemMonitor", // **重要**: 这必须与前端视图组件的文件名 (SystemMonitor.vue) 匹配
                    Icon = "DesktopOutline", // 我为您选择了一个合适的图标
                    ParentId = SystemManagementMenuId, // 假设它属于“系统管理”菜单
                    Order = 140, // 假设顺序在“用户管理”之后
                    Type = MenuType.Route,
                    PermissionCode = "system:monitor:view", // 新的权限码
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
