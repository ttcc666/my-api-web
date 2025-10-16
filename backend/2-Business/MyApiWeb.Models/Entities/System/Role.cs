using SqlSugar;
using System.ComponentModel.DataAnnotations;
using MyApiWeb.Models.Entities.Common;

namespace MyApiWeb.Models.Entities.System
{
    /// <summary>
    /// 角色实体类
    /// </summary>
    [SugarTable("Sys_Roles")]
    public class Role : EntityBase
    {
        /// <summary>
        /// 角色名称
        /// </summary>
        [SugarColumn(ColumnName = "F_Name", Length = 50, IsNullable = false)]
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 角色描述
        /// </summary>
        [SugarColumn(ColumnName = "F_Description", Length = 200, IsNullable = true)]
        public string? Description { get; set; }

        /// <summary>
        /// 是否为系统角色（如超级管理员，不可删除）
        /// </summary>
        [SugarColumn(ColumnName = "F_IsSystem", IsNullable = false)]
        public bool IsSystem { get; set; } = false;

        /// <summary>
        /// 是否启用
        /// </summary>
        [SugarColumn(ColumnName = "F_IsEnabled", IsNullable = false)]
        public bool IsEnabled { get; set; } = true;
    }
}
