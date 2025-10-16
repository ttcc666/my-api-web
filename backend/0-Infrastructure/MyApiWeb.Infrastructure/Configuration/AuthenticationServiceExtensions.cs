using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MyApiWeb.Models.DTOs;
using System.Text;
using System.Text.Json;

namespace MyApiWeb.Infrastructure.Configuration
{
    /// <summary>
    /// JWT 认证和授权服务配置扩展
    /// </summary>
    public static class AuthenticationServiceExtensions
    {
        /// <summary>
        /// 添加 JWT 认证和授权服务
        /// </summary>
        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // 配置 JWT 认证
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["JwtSettings:Issuer"],
                    ValidAudience = configuration["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["JwtSettings:Secret"]
                            ?? throw new InvalidOperationException("JWT Secret 未配置"))),
                    ClockSkew = TimeSpan.Zero
                };

                // 处理因令牌过期导致的认证失败事件,方便前端识别
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            context.Response.Headers["Token-Expired"] = "true";
                        }
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        if (context.Response.HasStarted)
                        {
                            return Task.CompletedTask;
                        }

                        if (context.AuthenticateFailure is SecurityTokenExpiredException &&
                            !context.Response.Headers.ContainsKey("Token-Expired"))
                        {
                            context.Response.Headers["Token-Expired"] = "true";
                        }

                        // 对 SignalR 握手/实时连接请求不自定义响应，让框架返回标准 401 以保证连接协商正常
                        var path = context.HttpContext.Request.Path.Value ?? string.Empty;
                        var hubBasePath = configuration["SignalR:HubPathBase"] ?? "/hubs";
                        var isSignalR = path.StartsWith(hubBasePath, System.StringComparison.OrdinalIgnoreCase)
                                        || path.Contains("/negotiate", System.StringComparison.OrdinalIgnoreCase)
                                        || context.HttpContext.WebSockets.IsWebSocketRequest
                                        || context.Request.Headers["Accept"].ToString().Contains("text/event-stream", System.StringComparison.OrdinalIgnoreCase);

                        if (isSignalR)
                        {
                            // 不处理响应，保持默认的 401
                            return Task.CompletedTask;
                        }

                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status200OK;
                        context.Response.ContentType = "application/json";

                        var response = new ApiResponse<object?>(
                            false,
                            StatusCodes.Status401Unauthorized,
                            string.IsNullOrWhiteSpace(context.ErrorDescription) ? "未授权或令牌无效" : context.ErrorDescription,
                            null);

                        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
                    },
                    OnForbidden = context =>
                    {
                        if (context.Response.HasStarted)
                        {
                            return Task.CompletedTask;
                        }

                        // 对 SignalR 握手/实时连接请求不自定义响应，让框架返回标准 403 以保证连接协商正常
                        var path = context.HttpContext.Request.Path.Value ?? string.Empty;
                        var hubBasePath = configuration["SignalR:HubPathBase"] ?? "/hubs";
                        var isSignalR = path.StartsWith(hubBasePath, System.StringComparison.OrdinalIgnoreCase)
                                        || path.Contains("/negotiate", System.StringComparison.OrdinalIgnoreCase)
                                        || context.HttpContext.WebSockets.IsWebSocketRequest
                                        || context.Request.Headers["Accept"].ToString().Contains("text/event-stream", System.StringComparison.OrdinalIgnoreCase);

                        if (isSignalR)
                        {
                            // 不处理响应，保持默认的 403
                            return Task.CompletedTask;
                        }

                        context.Response.StatusCode = StatusCodes.Status200OK;
                        context.Response.ContentType = "application/json";

                        var response = new ApiResponse<object?>(
                            false,
                            StatusCodes.Status403Forbidden,
                            "无访问权限",
                            null);

                        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
                    }
                };
            });

            // 添加授权服务
            services.AddAuthorization();

            return services;
        }
    }
}