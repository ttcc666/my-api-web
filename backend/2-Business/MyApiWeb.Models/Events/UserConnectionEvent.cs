namespace MyApiWeb.Models.Events
{
    /// <summary>
    /// 用户连接事件类型
    /// </summary>
    public enum ConnectionEventType
    {
        /// <summary>
        /// 用户上线
        /// </summary>
        Connected,

        /// <summary>
        /// 用户下线
        /// </summary>
        Disconnected,

        /// <summary>
        /// 心跳更新
        /// </summary>
        Heartbeat
    }

    /// <summary>
    /// 用户连接事件消息
    /// 用于通过 CAP 事件总线传递用户连接状态变化
    /// </summary>
    public class UserConnectionEvent
    {
        /// <summary>
        /// 事件类型
        /// </summary>
        public ConnectionEventType EventType { get; set; }

        /// <summary>
        /// SignalR 连接 ID
        /// </summary>
        public string ConnectionId { get; set; } = string.Empty;

        /// <summary>
        /// 用户 ID
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 用户名
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// 客户端 IP 地址
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// 用户代理 (浏览器信息)
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// 所在房间/分组
        /// </summary>
        public string? Room { get; set; }

        /// <summary>
        /// 事件发生时间
        /// </summary>
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;

        /// <summary>
        /// 断开连接原因 (仅在 Disconnected 事件时有值)
        /// </summary>
        public string? DisconnectReason { get; set; }
    }
}
