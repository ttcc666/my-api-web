using DotNetCore.CAP;
using Microsoft.Extensions.Logging;
using MyApiWeb.Models.Events;
using MyApiWeb.Services.Interfaces;

namespace MyApiWeb.Services.Subscribers
{
    /// <summary>
    /// 在线用户连接事件订阅者
    /// 通过 CAP 订阅用户连接/断开事件并处理业务逻辑
    /// </summary>
    public class OnlineUserEventSubscriber : ICapSubscribe
    {
        private readonly IOnlineUserService _onlineUserService;
        private readonly ILogger<OnlineUserEventSubscriber> _logger;

        public OnlineUserEventSubscriber(
            IOnlineUserService onlineUserService,
            ILogger<OnlineUserEventSubscriber> logger)
        {
            _onlineUserService = onlineUserService;
            _logger = logger;
        }

        /// <summary>
        /// 订阅用户连接事件
        /// </summary>
        /// <param name="event">连接事件</param>
        [CapSubscribe("user.connection.event")]
        public async Task HandleUserConnectionEvent(UserConnectionEvent @event)
       {
            try
            {
                switch (@event.EventType)
                {
                    case ConnectionEventType.Connected:
                        await HandleConnectedEvent(@event);
                        break;

                    case ConnectionEventType.Disconnected:
                        await HandleDisconnectedEvent(@event);
                        break;

                    case ConnectionEventType.Heartbeat:
                        await HandleHeartbeatEvent(@event);
                        break;

                    default:
                        _logger.LogWarning("未知的连接事件类型: {EventType}", @event.EventType);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理用户连接事件失败: EventType={EventType}, UserId={UserId}, ConnectionId={ConnectionId}",
                    @event.EventType, @event.UserId, @event.ConnectionId);
            }
        }

        /// <summary>
        /// 处理用户上线事件
        /// </summary>
        private async Task HandleConnectedEvent(UserConnectionEvent @event)
        {
            await _onlineUserService.RecordUserConnectedAsync(
                @event.ConnectionId,
                @event.UserId,
                @event.Username,
                @event.IpAddress,
                @event.UserAgent,
                @event.Room);

            _logger.LogInformation(
                "用户上线事件已处理: UserId={UserId}, ConnectionId={ConnectionId}",
                @event.UserId, @event.ConnectionId);
        }

        /// <summary>
        /// 处理用户下线事件
        /// </summary>
        private async Task HandleDisconnectedEvent(UserConnectionEvent @event)
        {
            await _onlineUserService.RecordUserDisconnectedAsync(@event.ConnectionId);

            _logger.LogInformation(
                "用户下线事件已处理: UserId={UserId}, ConnectionId={ConnectionId}, Reason={Reason}",
                @event.UserId, @event.ConnectionId, @event.DisconnectReason ?? "正常断开");
        }

        /// <summary>
        /// 处理心跳事件
        /// </summary>
        private async Task HandleHeartbeatEvent(UserConnectionEvent @event)
        {
            var updated = await _onlineUserService.UpdateHeartbeatAsync(@event.ConnectionId);

            if (!updated)
            {
                _logger.LogWarning(
                    "心跳更新失败,连接可能已不存在: ConnectionId={ConnectionId}",
                    @event.ConnectionId);
            }
        }
    }
}
