using Lazy.Captcha.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MyApiWeb.Infrastructure.Configuration;

/// <summary>
/// Captcha 服务扩展
/// </summary>
public static class CaptchaServiceExtensions
{
    /// <summary>
    /// 添加验证码服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <returns></returns>
    public static IServiceCollection AddCaptchaServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 配置验证码选项
        var captchaConfig = configuration.GetSection("Captcha");

        services.AddCaptcha(captchaConfig);

        return services;
    }
}