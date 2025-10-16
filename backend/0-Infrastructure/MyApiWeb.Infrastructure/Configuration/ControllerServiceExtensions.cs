using Microsoft.Extensions.DependencyInjection;

namespace MyApiWeb.Infrastructure.Configuration
{
    /// <summary>
    /// 控制器服务配置扩展
    /// </summary>
    public static class ControllerServiceExtensions
    {
        /// <summary>
        /// 添加自定义控制器配置
        /// </summary>
        public static IServiceCollection AddCustomControllers(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    // 禁用自动模型验证过滤器,允许手动处理验证
                    options.SuppressModelStateInvalidFilter = true;
                });

            return services;
        }
    }
}