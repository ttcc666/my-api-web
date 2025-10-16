using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApiWeb.Infrastructure.Helpers;
using MyApiWeb.Models.DTOs;
using MyApiWeb.Services.Interfaces;

namespace MyApiWeb.Api.Controllers
{
    /// <summary>
    /// 令牌管理控制器
    /// </summary>
    /// <remarks>
    /// 提供 JWT 令牌的刷新和吊销功能。
    /// 用于实现无缝的用户会话管理和安全的登出机制。
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly ILogger<TokenController> _logger;

        /// <summary>
        /// 初始化令牌管理控制器
        /// </summary>
        /// <param name="tokenService">令牌服务</param>
        /// <param name="logger">日志记录器</param>
        public TokenController(ITokenService tokenService, ILogger<TokenController> logger)
        {
            _tokenService = tokenService;
            _logger = logger;
        }

        /// <summary>
        /// 刷新 JWT 令牌
        /// </summary>
        /// <param name="request">刷新令牌请求 (包含 RefreshToken)</param>
        /// <returns>新的访问令牌和刷新令牌</returns>
        /// <response code="200">令牌刷新成功,返回新的令牌对</response>
        /// <response code="401">刷新令牌无效、已过期或已被吊销</response>
        /// <response code="500">服务器内部错误</response>
        /// <remarks>
        /// 当 Access Token 即将过期或已过期时,使用此接口获取新的令牌对。
        ///
        /// **安全机制**:
        /// - 使用令牌轮换 (Token Rotation),旧的 Refresh Token 会立即失效
        /// - 每次刷新都会生成全新的 Access Token 和 Refresh Token
        /// - 防止令牌重放攻击
        ///
        /// **使用场景**:
        /// - 前端在 Access Token 过期前主动刷新 (推荐)
        /// - 收到 401 响应后尝试刷新令牌
        ///
        /// **示例请求**:
        /// ```json
        /// {
        ///   "refreshToken": "your_refresh_token_here"
        /// }
        /// ```
        /// </remarks>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<TokenDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            try
            {
                var (isSuccess, error, newTokens) = await _tokenService.RefreshTokenAsync(request.RefreshToken);
                if (!isSuccess)
                {
                    return ApiResultHelper.Error(error, 401);
                }
                return ApiResultHelper.Success(newTokens, "令牌刷新成功");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "刷新令牌时发生错误");
                return ApiResultHelper.Error("服务器内部错误");
            }
        }

        /// <summary>
        /// 用户登出 (吊销刷新令牌)
        /// </summary>
        /// <param name="request">登出请求 (包含 RefreshToken)</param>
        /// <returns>登出结果</returns>
        /// <response code="200">登出成功,令牌已吊销</response>
        /// <response code="401">需要有效的 Access Token 进行认证</response>
        /// <remarks>
        /// 将刷新令牌标记为已吊销,使其无法再用于刷新。
        ///
        /// **认证要求**:
        /// - 需要在请求头中携带有效的 Access Token
        /// - 确保操作者身份合法
        ///
        /// **安全说明**:
        /// - 即使令牌吊销失败 (如令牌已失效),也会返回成功响应
        /// - 客户端应同时清除本地存储的 Access Token 和 Refresh Token
        /// - Access Token 会在过期时间后自然失效
        ///
        /// **最佳实践**:
        /// - 登出后应跳转到登录页面
        /// - 清除所有本地用户状态和缓存
        ///
        /// **示例请求**:
        /// ```json
        /// {
        ///   "refreshToken": "your_refresh_token_here"
        /// }
        /// ```
        /// </remarks>
        [HttpPost("logout")]
        [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto request)
        {
            var result = await _tokenService.RevokeTokenAsync(request.RefreshToken);
            if (!result)
            {
                // 即便吊销失败（可能令牌已失效），也应告知前端登出成功
                _logger.LogWarning("尝试吊销一个无效或已吊销的令牌: {RefreshToken}", request.RefreshToken);
            }
            return ApiResultHelper.SuccessMessage("登出成功");
        }
    }

    /// <summary>
    /// 刷新令牌请求 DTO
    /// </summary>
    /// <remarks>
    /// 用于令牌刷新和登出接口的请求参数。
    /// </remarks>
    public class RefreshTokenRequestDto
    {
        /// <summary>
        /// 刷新令牌字符串
        /// </summary>
        /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
        public required string RefreshToken { get; set; }
    }
}