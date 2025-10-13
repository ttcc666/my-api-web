using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace MyApiWeb.Models.Entities
{
    /// <summary>
    /// 角色实体类
    /// </summary>
    [SugarTable("Roles")]
    public class Role : EntityBase
    {
        /// <summary>
        /// 角色名称
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 角色描述
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string? Description { get; set; }

        /// <summary>
        /// 是否为系统角色（如超级管理员，不可删除）
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public bool IsSystem { get; set; } = false;

        /// <summary>
        /// 是否启用
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public bool IsEnabled { get; set; } = true;
    }
}