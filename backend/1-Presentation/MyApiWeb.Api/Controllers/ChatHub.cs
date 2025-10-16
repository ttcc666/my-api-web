using DotNetCore.CAP;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MyApiWeb.Models.Events;
using MyApiWeb.Repository;

namespace MyApiWeb.Api.Controllers
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;
        private readonly ICapPublisher _capPublisher;
        private readonly SqlSugarDbContext _dbContext;

        public ChatHub(
            ILogger<ChatHub> logger,
            ICapPublisher capPublisher,
            SqlSugarDbContext dbContext)
        {
            _logger = logger;
            _capPublisher = capPublisher;
            _dbContext = dbContext;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new HubException("未认证用户或令牌无效，无法建立连接。");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");

            var httpContext = Context.GetHttpContext();
            var room = httpContext?.Request.Query["room"].ToString();
            if (!string.IsNullOrWhiteSpace(room))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"room:{room}");
                _logger.LogInformation("User {UserId} joined room {Room}", userId, room);
            }

            // 获取客户端信息
            var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();

            // 获取用户名
            var username = Context.User?.Identity?.Name;

            // 发布用户上线事件
            await _capPublisher.PublishAsync("user.connection.event", new UserConnectionEvent
            {
                EventType = ConnectionEventType.Connected,
                ConnectionId = Context.ConnectionId,
                UserId = userId,
                Username = username,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Room = room,
                Timestamp = DateTimeOffset.Now
            });

            _logger.LogInformation("User {UserId} connected with connectionId {ConnectionId}", userId, Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            _logger.LogInformation("User {UserId} disconnected. ConnectionId={ConnectionId} Error={Error}", userId, Context.ConnectionId, exception?.Message);

            if (!string.IsNullOrWhiteSpace(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user:{userId}");

                // 发布用户下线事件
                await _capPublisher.PublishAsync("user.connection.event", new UserConnectionEvent
                {
                    EventType = ConnectionEventType.Disconnected,
                    ConnectionId = Context.ConnectionId,
                    UserId = userId,
                    Timestamp = DateTimeOffset.Now,
                    DisconnectReason = exception?.Message
                });
            }

            await base.OnDisconnectedAsync(exception);
        }

        public Task SendToAll(string message)
        {
            var userId = Context.UserIdentifier ?? "anonymous";
            var payload = new
            {
                type = "broadcast",
                from = userId,
                message,
                timestamp = DateTimeOffset.UtcNow
            };
            return Clients.All.SendAsync("receive", payload);
        }

        public Task SendToUser(string targetUserId, string message)
        {
            var fromUserId = Context.UserIdentifier ?? "anonymous";
            var payload = new
            {
                type = "direct",
                from = fromUserId,
                to = targetUserId,
                message,
                timestamp = DateTimeOffset.UtcNow
            };
            return Clients.Group($"user:{targetUserId}").SendAsync("receive", payload);
        }

        public Task SendToGroup(string room, string message)
        {
            var userId = Context.UserIdentifier ?? "anonymous";
            var payload = new
            {
                type = "room",
                room,
                from = userId,
                message,
                timestamp = DateTimeOffset.UtcNow
            };
            return Clients.Group($"room:{room}").SendAsync("receive", payload);
        }

        public async Task JoinGroup(string room)
        {
            var userId = Context.UserIdentifier;
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new HubException("未认证用户或令牌无效。");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"room:{room}");
            _logger.LogInformation("User {UserId} manually joined room {Room}", userId, room);

            await Clients.Group($"room:{room}").SendAsync("system", new
            {
                type = "system",
                room,
                message = $"用户 {userId} 已加入房间",
                timestamp = DateTimeOffset.UtcNow
            });
        }

        public async Task LeaveGroup(string room)
        {
            var userId = Context.UserIdentifier;
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"room:{room}");
            _logger.LogInformation("User {UserId} left room {Room}", userId, room);

            await Clients.Group($"room:{room}").SendAsync("system", new
            {
                type = "system",
                room,
                message = $"用户 {userId} 已离开房间",
                timestamp = DateTimeOffset.UtcNow
            });
        }

        public Task<object> Ping()
        {
            return Task.FromResult<object>(new
            {
                ok = true,
                userId = Context.UserIdentifier,
                connectionId = Context.ConnectionId,
                serverTime = DateTimeOffset.UtcNow
            });
        }

        /// <summary>
        /// 心跳方法,用于保持连接活跃状态
        /// 客户端应定期调用此方法 (建议每 1-3 分钟)
        /// </summary>
        public async Task<object> Heartbeat()
        {
            var userId = Context.UserIdentifier;

            if (!string.IsNullOrWhiteSpace(userId))
            {
                // 发布心跳事件
                await _capPublisher.PublishAsync("user.connection.event", new UserConnectionEvent
                {
                    EventType = ConnectionEventType.Heartbeat,
                    ConnectionId = Context.ConnectionId,
                    UserId = userId,
                    Timestamp = DateTimeOffset.Now
                });
            }

            return new
            {
                ok = true,
                userId,
                connectionId = Context.ConnectionId,
                serverTime = DateTimeOffset.UtcNow
            };
        }
    }
}
