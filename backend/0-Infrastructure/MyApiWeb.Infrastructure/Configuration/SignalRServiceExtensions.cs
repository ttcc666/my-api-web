using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace MyApiWeb.Infrastructure.Configuration
{
    /// <summary>
    /// SignalR 服务扩展，集成 JWT 认证并提供基于令牌的用户标识解析。
    /// </summary>
    public static class SignalRServiceExtensions
    {
        /// <summary>
        /// 添加 SignalR 并启用基于 JWT 的用户标识解析，同时支持从查询字符串读取令牌以适配 WebSocket/Server-Sent Events 握手。
        ///
        /// 使用约定：
        /// - 默认从查询字符串键 access_token 中读取令牌（用于 WebSocket 握手）。
        /// - 默认 Hub 路由前缀为 /hubs（仅在该前缀下读取查询令牌），可通过配置 SignalR:HubPathBase 覆盖。
        ///
        /// 配置示例：
        /// "SignalR": {
        ///   "HubPathBase": "/hubs"
        /// }
        /// </summary>
        public static IServiceCollection AddSignalRWithJwtSupport(
            this IServiceCollection services,
            IConfiguration? configuration = null)
        {
            // 注册 SignalR
            services.AddSignalR();

            // 注册基于 JWT 的用户标识解析器
            services.AddSingleton<IUserIdProvider, JwtUserIdProvider>();

            // 为 JwtBearer 添加从查询字符串读取令牌的能力，适配 SignalR 握手
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                var existingOnMessageReceived = options.Events?.OnMessageReceived;

                options.Events ??= new JwtBearerEvents();

                options.Events.OnMessageReceived = async context =>
                {
                    // 仅在 SignalR Hub 路径下尝试从查询字符串读取令牌
                    var hubBasePath = configuration?["SignalR:HubPathBase"] ?? "/hubs";
                    var path = context.HttpContext.Request.Path.Value ?? string.Empty;

                    // 查询字符串中的令牌键
                    var accessToken = context.Request.Query["access_token"].ToString();

                    if (!string.IsNullOrEmpty(accessToken) &&
                        path.StartsWith(hubBasePath, StringComparison.OrdinalIgnoreCase))
                    {
                        context.Token = accessToken;
                    }

                    // 保留原事件处理链
                    if (existingOnMessageReceived is not null && string.IsNullOrEmpty(context.Token))
                    {
                        await existingOnMessageReceived(context);
                    }
                };
            });

            return services;
        }
    }

    /// <summary>
    /// 基于 JWT 的 SignalR 用户标识提供器。
    /// 优先使用 NameIdentifier (sub/uid 作为备选)。
    /// </summary>
    public sealed class JwtUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            var user = connection.User;
            if (user is null) return null;

            // 优先使用标准 ClaimTypes.NameIdentifier，其次兼容常见 JWT 字段
            var userId =
                user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst("sub")?.Value
                ?? user.FindFirst("uid")?.Value;

            return string.IsNullOrWhiteSpace(userId) ? null : userId;
        }
    }
}