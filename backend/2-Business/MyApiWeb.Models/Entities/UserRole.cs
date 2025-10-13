using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace MyApiWeb.Models.Entities
{
    /// <summary>
    /// 用户角色关联实体类
    /// </summary>
    [SugarTable("UserRoles")]
    public class UserRole : EntityBase
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [SugarColumn(Length = 36, IsNullable = false)]
        [Required]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 角色ID
        /// </summary>
        [SugarColumn(Length = 36, IsNullable = false)]
        [Required]
        public string RoleId { get; set; } = string.Empty;

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
        /// 导航属性 - 角色
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public Role? Role { get; set; }
    }
}