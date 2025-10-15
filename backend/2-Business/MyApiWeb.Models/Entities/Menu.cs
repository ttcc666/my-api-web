using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace MyApiWeb.Models.Entities
{
    /// <summary>
    /// 菜单类型
    /// </summary>
    public enum MenuType
    {
        Directory = 0,
        Route = 1
    }

    /// <summary>
    /// 菜单实体
    /// </summary>
    [SugarTable("Menus")]
    public class Menu : EntityBase
    {
        /// <summary>
        /// 菜单编码（唯一）
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false, UniqueGroupNameList = new[] { "idx_menu_code" })]
        [Required]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 菜单标题
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false)]
        [Required]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 路由路径（用于前端跳转）
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string? RoutePath { get; set; }

        /// <summary>
        /// 路由名称（用于匹配前端路由 name）
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string? RouteName { get; set; }

        /// <summary>
        /// 图标名称（前端解析）
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string? Icon { get; set; }

        /// <summary>
        /// 上级菜单ID
        /// </summary>
        [SugarColumn(Length = 36, IsNullable = true)]
        public string? ParentId { get; set; }

        /// <summary>
        /// 排序号（越小越靠前）
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int Order { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 菜单类型
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public MenuType Type { get; set; } = MenuType.Route;

        /// <summary>
        /// 绑定的权限编码（如 user:view）
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string? PermissionCode { get; set; }

        /// <summary>
        /// 菜单描述
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string? Description { get; set; }
    }
}
