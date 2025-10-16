using System.ComponentModel.DataAnnotations;

namespace MyApiWeb.Models.DTOs
{
    /// <summary>
    /// 用户权限分配DTO
    /// </summary>
    public class AssignUserRolesDto
    {
        /// <summary>
        /// 角色ID列表
        /// </summary>
        [Required(ErrorMessage = "角色ID列表不能为空")]
        public List<string> RoleIds { get; set; } = new();
    }

    /// <summary>
    /// 用户直接权限分配DTO
    /// </summary>
    public class AssignUserPermissionsDto
    {
        /// <summary>
        /// 权限ID列表
        /// </summary>
        [Required(ErrorMessage = "权限ID列表不能为空")]
        public List<string> PermissionIds { get; set; } = new();
    }

    /// <summary>
    /// 用户权限信息DTO
    /// </summary>
    public class UserPermissionInfoDto
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// 用户角色列表
        /// </summary>
        public List<RoleDto> Roles { get; set; } = new();

        /// <summary>
        /// 用户直接权限列表
        /// </summary>
        public List<PermissionDto> DirectPermissions { get; set; } = new();

        /// <summary>
        /// 用户最终权限列表（角色权限 + 直接权限的并集）
        /// </summary>
        public List<PermissionDto> EffectivePermissions { get; set; } = new();
    }

    /// <summary>
    /// 用户权限检查结果DTO
    /// </summary>
    public class UserPermissionCheckDto
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 权限名称
        /// </summary>
        public string Permission { get; set; } = string.Empty;

        /// <summary>
        /// 是否拥有该权限
        /// </summary>
        public bool HasPermission { get; set; }

        /// <summary>
        /// 权限来源（角色或直接分配）
        /// </summary>
        public string Source { get; set; } = string.Empty;
    }

    /// <summary>
    /// 权限检查请求DTO
    /// </summary>
    public class CheckPermissionDto
    {
        /// <summary>
        /// 权限名称列表
        /// </summary>
        [Required(ErrorMessage = "权限名称列表不能为空")]
        public List<string> Permissions { get; set; } = new();
    }

    /// <summary>
    /// 当前用户信息摘要
    /// </summary>
    public class CurrentUserInfoDto
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// 拥有的权限名称列表
        /// </summary>
        public List<string> Permissions { get; set; } = new();

        /// <summary>
        /// 拥有的角色名称列表
        /// </summary>
        public List<string> Roles { get; set; } = new();
    }
}