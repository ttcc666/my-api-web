using SqlSugar;
using System.ComponentModel.DataAnnotations;
using MyApiWeb.Models.Entities.Common;

namespace MyApiWeb.Models.Entities.System
{
    /// <summary>
    /// 用户权限关联实体类（直接分配给用户的权限）
    /// </summary>
    [SugarTable("Sys_UserPermissions")]
    public class UserPermission : EntityBase
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [SugarColumn(ColumnName = "F_UserId", Length = 36, IsNullable = false)]
        [Required]
        public string UserId { get; set; } = string.Empty;

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
        /// 导航属性 - 用户
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public User? User { get; set; }

        /// <summary>
        /// 导航属性 - 权限
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public Permission? Permission { get; set; }
    }
}
