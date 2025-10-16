using Microsoft.Extensions.Logging;
using MyApiWeb.Infrastructure.Data.SeedConfigurations;
using MyApiWeb.Models.Entities;
using MyApiWeb.Repository;
using SqlSugar;

namespace MyApiWeb.Infrastructure.Data
{
    public class RbacDataSeeder : BaseDataSeeder<Permission>
    {
        public override string SeedName => "RbacData";

        public RbacDataSeeder(SqlSugarDbContext dbContext, ILogger<RbacDataSeeder> logger)
            : base(dbContext, logger) { }

        protected override async Task ClearDataAsync()
        {
            await DbContext.Db.Deleteable<RolePermission>().ExecuteCommandAsync();
            await DbContext.Db.Deleteable<UserRole>().ExecuteCommandAsync();
            await DbContext.Db.Deleteable<Permission>().ExecuteCommandAsync();
            await DbContext.Db.Deleteable<Role>().ExecuteCommandAsync();
        }

        protected override async Task SeedDataAsync()
        {
            await SeedPermissionsAsync();
            await SeedRolesAsync();
            await AssignPermissionsToSuperAdminAsync();
            await AssignSuperAdminRoleAsync();
        }

        private async Task SeedPermissionsAsync()
        {
            var count = await BulkInsertIfNotExistsAsync(
                PermissionSeedConfig.GetPermissions(),
                async p => await DbContext.Queryable<Permission>().Where(x => x.Name == p.Name).AnyAsync()
            );
            if (count > 0) Logger.LogInformation("创建了 {Count} 个权限", count);
        }

        private async Task SeedRolesAsync()
        {
            var count = await BulkInsertIfNotExistsAsync(
                RoleSeedConfig.GetRoles(),
                async r => await DbContext.Queryable<Role>().Where(x => x.Id == r.Id).AnyAsync()
            );
            if (count > 0) Logger.LogInformation("创建了 {Count} 个角色", count);
        }

        private async Task AssignPermissionsToSuperAdminAsync()
        {
            var allPermissions = await DbContext.Queryable<Permission>().ToListAsync();
            var existingIds = await DbContext.Queryable<RolePermission>()
                .Where(rp => rp.RoleId == RoleSeedConfig.SuperAdminRoleId)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            var newPermissions = allPermissions
                .Where(p => !existingIds.Contains(p.Id))
                .Select(p => new RolePermission
                {
                    RoleId = RoleSeedConfig.SuperAdminRoleId,
                    PermissionId = p.Id,
                    AssignedBy = "system",
                    AssignedTime = DateTimeOffset.Now
                }).ToList();

            if (newPermissions.Any())
            {
                await DbContext.Db.Insertable(newPermissions).ExecuteCommandAsync();
                Logger.LogInformation("为超级管理员分配了 {Count} 个权限", newPermissions.Count);
            }
        }

        private async Task AssignSuperAdminRoleAsync()
        {
            var admin = await DbContext.Queryable<User>().Where(u => u.Username == "admin").FirstAsync();
            if (admin == null) return;

            var hasRole = await DbContext.Queryable<UserRole>()
                .Where(ur => ur.UserId == admin.Id && ur.RoleId == RoleSeedConfig.SuperAdminRoleId)
                .AnyAsync();

            if (!hasRole)
            {
                await DbContext.Insertable(new UserRole
                {
                    UserId = admin.Id,
                    RoleId = RoleSeedConfig.SuperAdminRoleId,
                    AssignedBy = "system",
                    AssignedTime = DateTimeOffset.Now
                }).ExecuteCommandAsync();
                Logger.LogInformation("为管理员用户分配了超级管理员角色");
            }
        }
    }
}