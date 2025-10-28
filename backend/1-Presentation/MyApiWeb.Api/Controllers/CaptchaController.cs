using Lazy.Captcha.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApiWeb.Models.DTOs;
using MyApiWeb.Services.Interfaces.System;

namespace MyApiWeb.Api.Controllers;

/// <summary>
/// 验证码控制器
/// </summary>
[AllowAnonymous]
public class CaptchaController : ApiControllerBase
{
    private readonly ICaptcha _captcha;

    public CaptchaController(ICaptcha captcha)
    {
        _captcha = captcha;
    }

    /// <summary>
    /// 生成验证码
    /// </summary>
    /// <returns>验证码信息</returns>
    [HttpGet("generate")]
    public async Task<IActionResult> GenerateCaptcha()
    {
        try
        {
            var captchaId = Guid.NewGuid().ToString();
            var captchaCode = _captcha.Generate(captchaId);

            if (captchaCode == null)
            {
                return Error("生成验证码失败");
            }

            var result = new
            {
                CaptchaId = captchaId,
                Image = $"data:image/png;base64,{captchaCode.Base64}",
                ExpiryMinutes = 5 // 验证码5分钟过期
            };

            return Success(result);
        }
        catch (Exception ex)
        {
            return Error($"生成验证码异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 验证验证码
    /// </summary>
    /// <param name="request">验证请求</param>
    /// <returns>验证结果</returns>
    [HttpPost("validate")]
    public IActionResult ValidateCaptcha([FromBody] CaptchaValidateRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.CaptchaId) || string.IsNullOrWhiteSpace(request.Code))
            {
                return Error("验证码参数不能为空");
            }

            var isValid = _captcha.Validate(request.CaptchaId, request.Code);

            if (isValid)
            {
                return Success(new { IsValid = true });
            }
            else
            {
                return Error("验证码错误或已过期");
            }
        }
        catch (Exception ex)
        {
            return Error($"验证验证码异常: {ex.Message}");
        }
    }
}

/// <summary>
/// 验证码验证请求
/// </summary>
public class CaptchaValidateRequest
{
    /// <summary>
    /// 验证码ID
    /// </summary>
    public string CaptchaId { get; set; } = string.Empty;

    /// <summary>
    /// 验证码
    /// </summary>
    public string Code { get; set; } = string.Empty;
}