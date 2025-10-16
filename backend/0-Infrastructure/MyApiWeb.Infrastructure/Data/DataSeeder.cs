using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyApiWeb.Models.Entities.Common;
using MyApiWeb.Models.Entities.System;
using MyApiWeb.Models.Entities.Auth;
using MyApiWeb.Models.Entities.Hub;
using MyApiWeb.Repository;
using MyApiWeb.Services.Interfaces.System;

namespace MyApiWeb.Infrastructure.Data
{
    public static class DataSeeder
    {
        public static void Seed(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<RbacDataSeeder>>();
            var configuration = services.GetRequiredService<IConfiguration>();
            var dbContext = services.GetRequiredService<SqlSugarDbContext>();

            var enableSeeding = configuration.GetSection("DatabaseSettings:EnableDataSeeding").Get<bool?>() ?? true;
            var forceReseed = configuration.GetSection("DatabaseSettings:ForceReseedOnStartup").Get<bool?>() ?? false;

            if (!enableSeeding && !forceReseed)
            {
                logger.LogInformation("数据种子功能已禁用");
                return;
            }

            try
            {
                if (forceReseed)
                {
                    logger.LogWarning("强制重新执行种子数据...");
                    dbContext.Db.Deleteable<SeedHistory>().ExecuteCommand();
                }

                SeedAdminUser(services, dbContext, forceReseed, logger);
                SeedData<RbacDataSeeder>(services, dbContext, forceReseed, logger);
                SeedData<MenuDataSeeder>(services, dbContext, forceReseed, logger);

                logger.LogInformation("🎉 所有种子数据检查完成");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ 数据种子初始化失败");
                throw;
            }
        }

        private static void SeedAdminUser(IServiceProvider services, SqlSugarDbContext dbContext, bool forceReseed, ILogger logger)
        {
            const string seedName = "AdminUser";
            if (HasSeedExecuted(dbContext, seedName) && !forceReseed)
            {
                logger.LogInformation("⏭️ 跳过管理员用户初始化（已执行过）");
                return;
            }

            var userService = services.GetService<IUserService>();
            if (userService != null)
            {
                logger.LogInformation("开始初始化管理员用户...");
                CreateAdminUser(userService).Wait();
                MarkSeedAsExecuted(dbContext, seedName, "初始化管理员用户");
                logger.LogInformation("✅ 管理员用户初始化完成");
            }
        }

        private static void SeedData<T>(IServiceProvider services, SqlSugarDbContext dbContext, bool forceReseed, ILogger logger) where T : IDataSeeder
        {
            var seeder = services.GetService<T>();
            if (seeder == null) return;

            if (HasSeedExecuted(dbContext, seeder.SeedName) && !forceReseed)
            {
                logger.LogInformation("⏭️ 跳过 {Name} 初始化（已执行过）", seeder.SeedName);
                return;
            }

            logger.LogInformation("开始初始化 {Name}...", seeder.SeedName);
            seeder.SeedAsync(forceReseed).Wait();
            MarkSeedAsExecuted(dbContext, seeder.SeedName, $"初始化 {seeder.SeedName}");
            logger.LogInformation("✅ {Name} 初始化完成", seeder.SeedName);
        }

        private static bool HasSeedExecuted(SqlSugarDbContext dbContext, string seedName) =>
            dbContext.Queryable<SeedHistory>().Where(s => s.SeedName == seedName).Any();

        private static void MarkSeedAsExecuted(SqlSugarDbContext dbContext, string seedName, string remarks) =>
            dbContext.Insertable(new SeedHistory
            {
                SeedName = seedName,
                ExecutedAt = DateTimeOffset.Now,
                ExecutedBy = "System",
                Remarks = remarks
            }).ExecuteCommand();

        private static async Task CreateAdminUser(IUserService userService)
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
