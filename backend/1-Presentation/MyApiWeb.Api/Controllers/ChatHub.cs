using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace MyApiWeb.Api.Controllers
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(ILogger<ChatHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new HubException("未认证用户或令牌无效，无法建立连接。");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");

            var room = Context.GetHttpContext()?.Request.Query["room"].ToString();
            if (!string.IsNullOrWhiteSpace(room))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"room:{room}");
                _logger.LogInformation("User {UserId} joined room {Room}", userId, room);
            }

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
    }
}