using MyApiWeb.Models.Entities;

namespace MyApiWeb.Infrastructure.Data.SeedConfigurations
{
    public static class RoleSeedConfig
    {
        public const string SuperAdminRoleId = "super-admin-role-id";
        public const string AdminRoleId = "admin-role-id";
        public const string UserRoleId = "user-role-id";

        public static List<Role> GetRoles() => new()
        {
            new() { Id = SuperAdminRoleId, Name = "超级管理员", Description = "系统超级管理员，拥有所有权限", IsSystem = true, IsEnabled = true },
            new() { Id = AdminRoleId, Name = "管理员", Description = "系统管理员，拥有大部分管理权限", IsSystem = false, IsEnabled = true },
            new() { Id = UserRoleId, Name = "普通用户", Description = "普通用户，拥有基础权限", IsSystem = false, IsEnabled = true }
        };
    }
}