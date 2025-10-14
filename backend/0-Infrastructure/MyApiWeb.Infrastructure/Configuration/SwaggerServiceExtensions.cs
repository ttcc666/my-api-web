using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace MyApiWeb.Infrastructure.Configuration
{
    /// <summary>
    /// Swagger/OpenAPI 服务配置扩展
    /// </summary>
    public static class SwaggerServiceExtensions
    {
        /// <summary>
        /// 添加 Swagger/OpenAPI 文档配置
        /// </summary>
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "My API Web",
                    Version = "v1",
                    Description = "基于 .NET 9 + SqlSugar + Vue 3 的三层架构 Web API"
                });

                // 添加 JWT 认证支持
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            return services;
        }
    }
}
