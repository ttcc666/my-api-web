using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace MyApiWeb.Models.Entities
{
    /// <summary>
    /// 权限实体类
    /// </summary>
    [SugarTable("Sys_Permissions")]
    public class Permission : EntityBase
    {
        /// <summary>
        /// 权限名称（如：user:create, user:read, role:manage）
        /// </summary>
        [SugarColumn(ColumnName = "F_Name", Length = 100, IsNullable = false)]
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 权限显示名称
        /// </summary>
        [SugarColumn(ColumnName = "F_DisplayName", Length = 100, IsNullable = false)]
        [Required]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 权限描述
        /// </summary>
        [SugarColumn(ColumnName = "F_Description", Length = 200, IsNullable = true)]
        public string? Description { get; set; }

        /// <summary>
        /// 权限分组（如：用户管理、角色管理、系统设置）
        /// </summary>
        [SugarColumn(ColumnName = "F_Group", Length = 50, IsNullable = true)]
        public string? Group { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [SugarColumn(ColumnName = "F_IsEnabled", IsNullable = false)]
        public bool IsEnabled { get; set; } = true;
    }
}
