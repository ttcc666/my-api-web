using Microsoft.Extensions.Logging;
using MyApiWeb.Infrastructure.Data.SeedConfigurations;
using MyApiWeb.Models.Entities;
using MyApiWeb.Repository;

namespace MyApiWeb.Infrastructure.Data
{
    public class MenuDataSeeder : BaseDataSeeder<Menu>
    {
        public override string SeedName => "MenuData";

        public MenuDataSeeder(SqlSugarDbContext dbContext, ILogger<MenuDataSeeder> logger)
            : base(dbContext, logger) { }

        protected override async Task SeedDataAsync()
        {
            var count = await BulkInsertIfNotExistsAsync(
                MenuSeedConfig.GetMenus(),
                async m => await DbContext.Queryable<Menu>().Where(x => x.Code == m.Code).AnyAsync()
            );
            if (count > 0) Logger.LogInformation("创建了 {Count} 个菜单", count);
        }
    }
}