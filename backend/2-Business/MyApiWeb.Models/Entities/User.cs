using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace MyApiWeb.Models.Entities
{
    /// <summary>
    /// 用户实体类
    /// </summary>
    [SugarTable("Users")]
    public class User : EntityBase
    {
        /// <summary>
        /// 用户名
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        [Required]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// 邮箱
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false)]
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 密码哈希
        /// </summary>
        [SugarColumn(Length = 255, IsNullable = false)]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// 真实姓名
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string? RealName { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true)]
        public string? Phone { get; set; }

        /// <summary>
        /// 是否激活
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 更新时间
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? UpdatedTime { get; set; }

        /// <summary>
        /// 最后登录时间
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? LastLoginTime { get; set; }
    }
}