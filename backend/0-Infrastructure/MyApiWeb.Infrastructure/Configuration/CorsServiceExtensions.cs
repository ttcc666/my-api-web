using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MyApiWeb.Infrastructure.Configuration
{
    /// <summary>
    /// CORS 跨域服务配置扩展
    /// </summary>
    public static class CorsServiceExtensions
    {
        /// <summary>
        /// 添加自定义 CORS 配置(根据环境自动选择策略)
        /// </summary>
        public static IServiceCollection AddCustomCors(
            this IServiceCollection services,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            services.AddCors(options =>
            {
                if (environment.IsDevelopment())
                {
                    // 开发环境：允许本地前端开发服务器
                    options.AddPolicy("DevelopmentPolicy", policy =>
                    {
                        policy.WithOrigins(
                                "http://localhost:3000",    // Vue 开发服务器
                                "http://localhost:5173",    // Vite 开发服务器
                                "http://127.0.0.1:3000",
                                "http://127.0.0.1:5173"
                              )
                              .AllowAnyMethod()
                              .AllowAnyHeader()
                              .AllowCredentials()
                              .SetIsOriginAllowedToAllowWildcardSubdomains();
                    });
                }
                else
                {
                    // 生产环境：严格的域名白名单
                    var allowedOrigins = configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>()
                                       ?? new[] { "https://yourdomain.com" };

                    options.AddPolicy("ProductionPolicy", policy =>
                    {
                        policy.WithOrigins(allowedOrigins)
                              .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                              .WithHeaders("Content-Type", "Authorization", "X-Requested-With")
                              .AllowCredentials()
                              .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
                    });
                }
            });

            return services;
        }
    }
}