using SqlSugar;
using System;

namespace MyApiWeb.Models.Entities
{
    [SugarTable("RefreshTokens")]
    public class RefreshToken : EntityBase
    {
        [SugarColumn(IsPrimaryKey = true)]
        public override string Id { get; set; } = Guid.NewGuid().ToString();

        public string UserId { get; set; }

        [SugarColumn(Length = 512)]
        public string Token { get; set; }

        /// <summary>
        /// 关联的 Access Token 的 JWT ID
        /// </summary>
        public string JwtId { get; set; }

        /// <summary>
        /// 是否已被使用（用于实现令牌轮换）
        /// </summary>
        public bool IsUsed { get; set; }

        /// <summary>
        /// 是否已被吊销（用于实现登出）
        /// </summary>
        public bool IsRevoked { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        [Navigate(NavigateType.OneToOne, nameof(UserId))]
        public User User { get; set; }
    }
}