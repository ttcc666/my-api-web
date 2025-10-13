using System.ComponentModel.DataAnnotations;

namespace MyApiWeb.Models.DTOs
{
    /// <summary>
    /// 权限数据传输对象
    /// </summary>
    public class PermissionDto
    {
        /// <summary>
        /// 权限ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 权限名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 权限显示名称
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 权限描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 权限分组
        /// </summary>
        public string? Group { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTimeOffset CreationTime { get; set; }
    }

    /// <summary>
    /// 创建权限请求DTO
    /// </summary>
    public class CreatePermissionDto
    {
        /// <summary>
        /// 权限名称
        /// </summary>
        [Required(ErrorMessage = "权限名称不能为空")]
        [StringLength(100, ErrorMessage = "权限名称长度不能超过100个字符")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 权限显示名称
        /// </summary>
        [Required(ErrorMessage = "权限显示名称不能为空")]
        [StringLength(100, ErrorMessage = "权限显示名称长度不能超过100个字符")]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 权限描述
        /// </summary>
        [StringLength(200, ErrorMessage = "权限描述长度不能超过200个字符")]
        public string? Description { get; set; }

        /// <summary>
        /// 权限分组
        /// </summary>
        [StringLength(50, ErrorMessage = "权限分组长度不能超过50个字符")]
        public string? Group { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;
    }

    /// <summary>
    /// 更新权限请求DTO
    /// </summary>
    public class UpdatePermissionDto
    {
        /// <summary>
        /// 权限名称
        /// </summary>
        [Required(ErrorMessage = "权限名称不能为空")]
        [StringLength(100, ErrorMessage = "权限名称长度不能超过100个字符")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 权限显示名称
        /// </summary>
        [Required(ErrorMessage = "权限显示名称不能为空")]
        [StringLength(100, ErrorMessage = "权限显示名称长度不能超过100个字符")]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 权限描述
        /// </summary>
        [StringLength(200, ErrorMessage = "权限描述长度不能超过200个字符")]
        public string? Description { get; set; }

        /// <summary>
        /// 权限分组
        /// </summary>
        [StringLength(50, ErrorMessage = "权限分组长度不能超过50个字符")]
        public string? Group { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; }
    }

    /// <summary>
    /// 权限分组DTO
    /// </summary>
    public class PermissionGroupDto
    {
        /// <summary>
        /// 分组名称
        /// </summary>
        public string Group { get; set; } = string.Empty;

        /// <summary>
        /// 该分组下的权限列表
        /// </summary>
        public List<PermissionDto> Permissions { get; set; } = new();
    }
}