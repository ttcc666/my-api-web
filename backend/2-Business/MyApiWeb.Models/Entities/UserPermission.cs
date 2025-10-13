using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace MyApiWeb.Models.Entities
{
    /// <summary>
    /// 用户权限关联实体类（直接分配给用户的权限）
    /// </summary>
    [SugarTable("UserPermissions")]
    public class UserPermission : EntityBase
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [SugarColumn(Length = 36, IsNullable = false)]
        [Required]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 权限ID
        /// </summary>
        [SugarColumn(Length = 36, IsNullable = false)]
        [Required]
        public string PermissionId { get; set; } = string.Empty;

        /// <summary>
        /// 分配时间
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTimeOffset AssignedTime { get; set; } = DateTimeOffset.Now;

        /// <summary>
        /// 分配者ID
        /// </summary>
        [SugarColumn(Length = 36, IsNullable = false)]
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