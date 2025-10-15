using System.ComponentModel.DataAnnotations;
using MyApiWeb.Models.Entities;

namespace MyApiWeb.Models.DTOs
{
    /// <summary>
    /// 菜单信息 DTO
    /// </summary>
    public class MenuDto
    {
        public string Id { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string? RoutePath { get; set; }

        public string? RouteName { get; set; }

        public string? Icon { get; set; }

        public string? ParentId { get; set; }

        public int Order { get; set; }

        public bool IsEnabled { get; set; }

        public MenuType Type { get; set; }

        public string? PermissionCode { get; set; }

        public string? Description { get; set; }

        public List<MenuDto> Children { get; set; } = new();
    }

    /// <summary>
    /// 创建菜单 DTO
    /// </summary>
    public class CreateMenuDto
    {
        [Required]
        [MaxLength(100)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? RoutePath { get; set; }

        [MaxLength(100)]
        public string? RouteName { get; set; }

        [MaxLength(100)]
        public string? Icon { get; set; }

        [MaxLength(36)]
        public string? ParentId { get; set; }

        [MaxLength(100)]
        public string? PermissionCode { get; set; }

        [MaxLength(200)]
        public string? Description { get; set; }

        [Range(0, int.MaxValue)]
        public int Order { get; set; }

        [Required]
        public MenuType Type { get; set; } = MenuType.Route;

        public bool IsEnabled { get; set; } = true;
    }

    /// <summary>
    /// 更新菜单 DTO
    /// </summary>
    public class UpdateMenuDto
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? RoutePath { get; set; }

        [MaxLength(100)]
        public string? RouteName { get; set; }

        [MaxLength(100)]
        public string? Icon { get; set; }

        [MaxLength(36)]
        public string? ParentId { get; set; }

        [MaxLength(100)]
        public string? PermissionCode { get; set; }

        [MaxLength(200)]
        public string? Description { get; set; }

        [Range(0, int.MaxValue)]
        public int Order { get; set; }

        [Required]
        public MenuType Type { get; set; } = MenuType.Route;

        public bool IsEnabled { get; set; } = true;
    }
}
