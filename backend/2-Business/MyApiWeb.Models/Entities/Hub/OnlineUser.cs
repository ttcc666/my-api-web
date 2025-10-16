using SqlSugar;
using MyApiWeb.Models.Entities.Common;

namespace MyApiWeb.Models.Entities.Hub
{
    /// <summary>
    /// 在线用户实体类
    /// 记录通过 SignalR 连接的在线用户信息
    /// </summary>
    [SugarTable("Hub_OnlineUsers")]
    public class OnlineUser : EntityBase
    {
        /// <summary>
        /// SignalR 连接 ID
        /// </summary>
        [SugarColumn(ColumnName = "F_ConnectionId", Length = 100, IsNullable = false)]
        public string ConnectionId { get; set; } = string.Empty;

        /// <summary>
        /// 用户 ID
        /// </summary>
        [SugarColumn(ColumnName = "F_UserId", Length = 100, IsNullable = false)]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 用户名
        /// </summary>
        [SugarColumn(ColumnName = "F_Username", Length = 50, IsNullable = true)]
        public string? Username { get; set; }

        /// <summary>
        /// 连接建立时间
        /// </summary>
        [SugarColumn(ColumnName = "F_ConnectedAt", IsNullable = false)]
        public DateTimeOffset ConnectedAt { get; set; } = DateTimeOffset.Now;

        /// <summary>
        /// 最后心跳时间
        /// </summary>
        [SugarColumn(ColumnName = "F_LastHeartbeatAt", IsNullable = false)]
        public DateTimeOffset LastHeartbeatAt { get; set; } = DateTimeOffset.Now;

        /// <summary>
        /// 客户端 IP 地址
        /// </summary>
        [SugarColumn(ColumnName = "F_IpAddress", Length = 50, IsNullable = true)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// 用户代理 (浏览器信息)
        /// </summary>
        [SugarColumn(ColumnName = "F_UserAgent", Length = 500, IsNullable = true)]
        public string? UserAgent { get; set; }

        /// <summary>
        /// 所在房间/分组
        /// </summary>
        [SugarColumn(ColumnName = "F_Room", Length = 100, IsNullable = true)]
        public string? Room { get; set; }

        /// <summary>
        /// 在线状态
        /// Online: 在线, Idle: 空闲, Offline: 离线
        /// </summary>
        [SugarColumn(ColumnName = "F_Status", Length = 20, IsNullable = false)]
        public string Status { get; set; } = "Online";

        /// <summary>
        /// 断开连接时间
        /// </summary>
        [SugarColumn(ColumnName = "F_DisconnectedAt", IsNullable = true)]
        public DateTimeOffset? DisconnectedAt { get; set; }
    }
}
