using System.ComponentModel.DataAnnotations;

namespace MyApiWeb.Models.DTOs
{
    /// <summary>
    /// 角色数据传输对象
    /// </summary>
    public class RoleDto
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 角色名称
        /// </summary>
        [Required(ErrorMessage = "角色名称不能为空")]
        [StringLength(50, ErrorMessage = "角色名称长度不能超过50个字符")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 角色描述
        /// </summary>
        [StringLength(200, ErrorMessage = "角色描述长度不能超过200个字符")]
        public string? Description { get; set; }

        /// <summary>
        /// 是否为系统角色
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTimeOffset CreationTime { get; set; }

        /// <summary>
        /// 权限列表
        /// </summary>
        public List<PermissionDto> Permissions { get; set; } = new();
    }

    /// <summary>
    /// 创建角色请求DTO
    /// </summary>
    public class CreateRoleDto
    {
        /// <summary>
        /// 角色名称
        /// </summary>
        [Required(ErrorMessage = "角色名称不能为空")]
        [StringLength(50, ErrorMessage = "角色名称长度不能超过50个字符")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 角色描述
        /// </summary>
        [StringLength(200, ErrorMessage = "角色描述长度不能超过200个字符")]
        public string? Description { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 权限ID列表
        /// </summary>
        public List<string> PermissionIds { get; set; } = new();
    }

    /// <summary>
    /// 更新角色请求DTO
    /// </summary>
    public class UpdateRoleDto
    {
        /// <summary>
        /// 角色名称
        /// </summary>
        [Required(ErrorMessage = "角色名称不能为空")]
        [StringLength(50, ErrorMessage = "角色名称长度不能超过50个字符")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 角色描述
        /// </summary>
        [StringLength(200, ErrorMessage = "角色描述长度不能超过200个字符")]
        public string? Description { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; }
    }

    /// <summary>
    /// 角色权限分配DTO
    /// </summary>
    public class AssignRolePermissionsDto
    {
        /// <summary>
        /// 权限ID列表
        /// </summary>
        [Required(ErrorMessage = "权限ID列表不能为空")]
        public List<string> PermissionIds { get; set; } = new();
    }
}