using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyApiWeb.Models.Entities;
using MyApiWeb.Repository;
using MyApiWeb.Services.Interfaces;
using SqlSugar;

namespace MyApiWeb.Infrastructure.Data
{
    /// <summary>
    /// 数据种子服务
    /// </summary>
    public static class DataSeeder
    {
        private const string AdminUserSeedName = "AdminUser";
        private const string RbacDataSeedName = "RbacData";
        private const string MenuDataSeedName = "MenuData";

        /// <summary>
        /// 初始化所有种子数据
        /// </summary>
        public static void Seed(WebApplication app)
        {
            using (var serviceScope = app.Services.CreateScope())
            {
                var services = serviceScope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<RbacDataSeeder>>();
                var configuration = services.GetRequiredService<IConfiguration>();
                var dbContext = services.GetRequiredService<SqlSugarDbContext>();

                // 检查配置是否启用种子数据
                var enableSeeding = configuration.GetSection("DatabaseSettings:EnableDataSeeding").Get<bool?>() ?? true;
                var forceReseed = configuration.GetSection("DatabaseSettings:ForceReseedOnStartup").Get<bool?>() ?? false;

                if (!enableSeeding && !forceReseed)
                {
                    logger.LogInformation("数据种子功能已禁用");
                    return;
                }

                try
                {
                    // 如果强制重新执行，先清除历史记录
                    if (forceReseed)
                    {
                        logger.LogWarning("强制重新执行种子数据...");
                        dbContext.Db.Deleteable<SeedHistory>().ExecuteCommand();
                    }

                    // 初始化管理员用户
                    if (!HasSeedExecuted(dbContext, AdminUserSeedName) || forceReseed)
                    {
                        var userService = services.GetService<IUserService>();
                        if (userService != null)
                        {
                            logger.LogInformation("开始初始化管理员用户...");
                            SeedAdminUser(userService).Wait();
                            MarkSeedAsExecuted(dbContext, AdminUserSeedName, "初始化管理员用户");
                            logger.LogInformation("✅ 管理员用户初始化完成");
                        }
                    }
                    else
                    {
                        logger.LogInformation("⏭️ 跳过管理员用户初始化（已执行过）");
                    }

                    // 初始化 RBAC 权限数据
                    if (!HasSeedExecuted(dbContext, RbacDataSeedName) || forceReseed)
                    {
                        var rbacSeeder = services.GetService<RbacDataSeeder>();
                        if (rbacSeeder != null)
                        {
                            logger.LogInformation("开始初始化 RBAC 权限数据...");
                            rbacSeeder.SeedAsync().Wait();
                            MarkSeedAsExecuted(dbContext, RbacDataSeedName, "初始化 RBAC 权限、角色和关联关系");
                            logger.LogInformation("✅ RBAC 数据初始化完成");
                        }
                    }
                    else
                    {
                        logger.LogInformation("⏭️ 跳过 RBAC 数据初始化（已执行过）");
                    }

                    // 初始化菜单数据
                    if (!HasSeedExecuted(dbContext, MenuDataSeedName) || forceReseed)
                    {
                        var menuSeeder = services.GetService<MenuDataSeeder>();
                        if (menuSeeder != null)
                        {
                            logger.LogInformation("开始初始化菜单数据...");
                            menuSeeder.SeedAsync().Wait();
                            MarkSeedAsExecuted(dbContext, MenuDataSeedName, "初始化前端菜单数据");
                            logger.LogInformation("✅ 菜单数据初始化完成");
                        }
                    }
                    else
                    {
                        logger.LogInformation("⏭️ 跳过菜单数据初始化（已执行过）");
                    }


                    logger.LogInformation("🎉 所有种子数据检查完成");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "❌ 数据种子初始化失败");
                    throw;
                }
            }
        }

        /// <summary>
        /// 检查种子数据是否已执行
        /// </summary>
        private static bool HasSeedExecuted(SqlSugarDbContext dbContext, string seedName)
        {
            return dbContext.Queryable<SeedHistory>()
                .Where(s => s.SeedName == seedName)
                .Any();
        }

        /// <summary>
        /// 标记种子数据为已执行
        /// </summary>
        private static void MarkSeedAsExecuted(SqlSugarDbContext dbContext, string seedName, string remarks)
        {
            dbContext.Insertable(new SeedHistory
            {
                SeedName = seedName,
                ExecutedAt = DateTimeOffset.Now,
                ExecutedBy = "System",
                Remarks = remarks
            }).ExecuteCommand();
        }

        /// <summary>
        /// 创建默认管理员用户
        /// </summary>
        private static async Task SeedAdminUser(IUserService userService)
        {
            if (!await userService.UsernameExistsAsync("admin"))
            {
                await userService.RegisterAsync(new Models.DTOs.UserRegisterDto
                {
                    Username = "admin",
                    Email = "admin@example.com",
                    Password = "123456",
                    RealName = "Administrator"
                });
            }
        }
    }
}
