using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApiWeb.Api.Helpers;
using MyApiWeb.Models.DTOs;
using MyApiWeb.Services.Interfaces;
using System.Threading.Tasks;

namespace MyApiWeb.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly ILogger<TokenController> _logger;

        public TokenController(ITokenService tokenService, ILogger<TokenController> logger)
        {
            _tokenService = tokenService;
            _logger = logger;
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<TokenDto>), StatusCodes.Status200OK)]
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

        [HttpPost("logout")]
        // 登出需要有效的 Access Token，以确认操作者身份
        [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
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

    // 可以放在 DTOs 文件夹下
    public class RefreshTokenRequestDto
    {
        public string RefreshToken { get; set; }
    }
}
