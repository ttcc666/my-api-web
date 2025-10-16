using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace MyApiWeb.Models.Entities
{
    /// <summary>
    /// 角色权限关联实体类
    /// </summary>
    [SugarTable("Sys_RolePermissions")]
    public class RolePermission : EntityBase
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        [SugarColumn(ColumnName = "F_RoleId", Length = 36, IsNullable = false)]
        [Required]
        public string RoleId { get; set; } = string.Empty;

        /// <summary>
        /// 权限ID
        /// </summary>
        [SugarColumn(ColumnName = "F_PermissionId", Length = 36, IsNullable = false)]
        [Required]
        public string PermissionId { get; set; } = string.Empty;

        /// <summary>
        /// 分配时间
        /// </summary>
        [SugarColumn(ColumnName = "F_AssignedTime", IsNullable = false)]
        public DateTimeOffset AssignedTime { get; set; } = DateTimeOffset.Now;

        /// <summary>
        /// 分配者ID
        /// </summary>
        [SugarColumn(ColumnName = "F_AssignedBy", Length = 36, IsNullable = false)]
        public string AssignedBy { get; set; } = string.Empty;

        /// <summary>
        /// 导航属性 - 角色
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public Role? Role { get; set; }

        /// <summary>
        /// 导航属性 - 权限
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public Permission? Permission { get; set; }
    }
}
