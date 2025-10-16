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
        /// 策略:清理该用户在同一设备的旧连接记录,只保留最新的连接
        /// 使用设备指纹(IP + UserAgent)识别同一设备
        /// </summary>
        private async Task HandleConnectedEvent(UserConnectionEvent @event)
        {
            // 1️⃣ 先清理该用户在同一设备的旧连接(相同 UserId + 相同 IP + 相同 UserAgent 但不同 ConnectionId)
            await CleanupUserOldConnections(@event.UserId, @event.ConnectionId, @event.IpAddress, @event.UserAgent);

            // 2️⃣ 记录新连接
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
        /// 清理用户的旧连接记录
        /// 当用户重新连接时(如刷新浏览器),将该用户在同一设备的旧连接物理删除
        /// 使用设备指纹(IP + UserAgent)识别同一设备,保留其他设备的连接
        /// </summary>
        private async Task CleanupUserOldConnections(
            string userId,
            string currentConnectionId,
            string? ipAddress,
            string? userAgent)
        {
            try
            {
                // 删除该用户在同一设备的旧连接(相同IP + 相同UserAgent,但不是当前连接的)
                var deletedCount = await _onlineUserService.DeleteUserOldConnectionsAsync(
                    userId,
                    currentConnectionId,
                    ipAddress,
                    userAgent);

                if (deletedCount > 0)
                {
                    _logger.LogInformation(
                        "已删除用户在同一设备的 {Count} 个旧连接记录: UserId={UserId}, IP={IpAddress}",
                        deletedCount, userId, ipAddress);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "清理用户旧连接失败: UserId={UserId}, CurrentConnectionId={CurrentConnectionId}",
                    userId, currentConnectionId);
                // 不抛出异常,让上线流程继续
            }
        }

        /// <summary>
        /// 处理用户下线事件
        /// 策略:
        /// - 如果是主动退出登录(DisconnectReason="Logout"),立即标记为离线
        /// - 如果是意外断开或刷新,不立即标记为离线,等待重连或超时清理
        /// </summary>
        private async Task HandleDisconnectedEvent(UserConnectionEvent @event)
        {
            var onlineUser = await _onlineUserService.GetByConnectionIdAsync(@event.ConnectionId);

            if (onlineUser != null)
            {
                // 判断是否是主动退出登录
                bool isLogout = string.Equals(@event.DisconnectReason, "Logout", StringComparison.OrdinalIgnoreCase);

                if (isLogout)
                {
                    // 主动退出登录:立即标记为离线
                    await _onlineUserService.RecordUserDisconnectedAsync(@event.ConnectionId);

                    _logger.LogInformation(
                        "用户主动退出登录,已标记为离线: UserId={UserId}, ConnectionId={ConnectionId}",
                        @event.UserId, @event.ConnectionId);
                }
                else
                {
                    // 意外断开或刷新浏览器:不立即标记为离线
                    // 原因:用户可能只是刷新浏览器,会在几秒内重新连接
                    // 真正的离线判断交给定时清理任务(基于心跳超时)
                    _logger.LogInformation(
                        "用户连接断开(暂不标记离线): UserId={UserId}, ConnectionId={ConnectionId}, Reason={Reason}",
                        @event.UserId, @event.ConnectionId, @event.DisconnectReason ?? "正常断开");
                }
            }
            else
            {
                _logger.LogWarning(
                    "尝试处理不存在的连接断开事件: ConnectionId={ConnectionId}",
                    @event.ConnectionId);
            }
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
