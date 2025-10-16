using SqlSugar;

namespace MyApiWeb.Models.Entities
{
    [SugarTable("Auth_RefreshTokens")]
    public class RefreshToken : EntityBase
    {
        [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
        public override string Id { get; set; } = Guid.NewGuid().ToString();

        [SugarColumn(ColumnName = "F_UserId")]
        public string UserId { get; set; } = string.Empty;

        [SugarColumn(ColumnName = "F_Token", Length = 512)]
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// 关联的 Access Token 的 JWT ID
        /// </summary>
        [SugarColumn(ColumnName = "F_JwtId")]
        public string JwtId { get; set; } = string.Empty;

        /// <summary>
        /// 是否已被使用（用于实现令牌轮换）
        /// </summary>
        [SugarColumn(ColumnName = "F_IsUsed")]
        public bool IsUsed { get; set; }

        /// <summary>
        /// 是否已被吊销（用于实现登出）
        /// </summary>
        [SugarColumn(ColumnName = "F_IsRevoked")]
        public bool IsRevoked { get; set; }

        [SugarColumn(ColumnName = "F_CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [SugarColumn(ColumnName = "F_ExpiresAt")]
        public DateTime ExpiresAt { get; set; }

        [Navigate(NavigateType.OneToOne, nameof(UserId))]
        public User? User { get; set; }
    }
}
