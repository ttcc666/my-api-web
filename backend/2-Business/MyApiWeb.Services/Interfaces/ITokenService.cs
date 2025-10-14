using MyApiWeb.Models.DTOs;
using MyApiWeb.Models.Entities;
using System.Threading.Tasks;

namespace MyApiWeb.Services.Interfaces
{
    /// <summary>
    /// JWT 令牌管理服务接口
    /// </summary>
    /// <remarks>
    /// 负责 JWT 访问令牌 (Access Token) 和刷新令牌 (Refresh Token) 的生成、刷新和吊销。
    /// 实现了令牌轮换 (Token Rotation) 机制以提升安全性。
    /// </remarks>
    public interface ITokenService
    {
        /// <summary>
        /// 为指定用户生成 JWT 访问令牌和刷新令牌
        /// </summary>
        /// <param name="user">用户实体对象</param>
        /// <returns>
        /// 包含访问令牌和刷新令牌的 DTO 对象
        /// </returns>
        /// <remarks>
        /// - Access Token: 用于 API 访问认证,有效期较短 (默认 30 分钟)
        /// - Refresh Token: 用于获取新的令牌对,有效期较长 (默认 7 天),存储在数据库中
        /// - 令牌中包含用户 ID、用户名等基本信息作为 Claims
        /// </remarks>
        Task<TokenDto> GenerateTokensAsync(User user);

        /// <summary>
        /// 使用刷新令牌获取新的令牌对 (令牌刷新)
        /// </summary>
        /// <param name="refreshToken">客户端提供的刷新令牌字符串</param>
        /// <returns>
        /// 元组包含三个值:
        /// - IsSuccess: 刷新是否成功
        /// - Error: 失败时的错误消息 (成功时为空字符串)
        /// - NewTokens: 新的令牌对 (失败时为 null)
        /// </returns>
        /// <remarks>
        /// <para><b>安全机制 - 令牌轮换 (Token Rotation):</b></para>
        /// - 每次使用刷新令牌时,旧令牌立即失效 (标记为已使用)
        /// - 同时生成新的访问令牌和刷新令牌返回给客户端
        /// - 防止令牌重放攻击 (Replay Attack)
        ///
        /// <para><b>验证失败场景:</b></para>
        /// - 令牌不存在或格式无效
        /// - 令牌已被使用或已被吊销
        /// - 令牌已过期
        /// - 关联的用户不存在
        /// </remarks>
        Task<(bool IsSuccess, string Error, TokenDto NewTokens)> RefreshTokenAsync(string refreshToken);

        /// <summary>
        /// 吊销刷新令牌 (用户登出)
        /// </summary>
        /// <param name="refreshToken">需要吊销的刷新令牌字符串</param>
        /// <returns>
        /// 是否吊销成功:
        /// - true: 令牌吊销成功
        /// - false: 令牌不存在或已被吊销
        /// </returns>
        /// <remarks>
        /// - 将令牌标记为 IsRevoked = true 和 IsUsed = true
        /// - 用于实现用户主动登出功能
        /// - 即使吊销失败 (令牌已失效),客户端也应清除本地令牌并视为登出成功
        /// - 不会抛出异常,通过返回值表示操作结果
        /// </remarks>
        Task<bool> RevokeTokenAsync(string refreshToken);
    }
}