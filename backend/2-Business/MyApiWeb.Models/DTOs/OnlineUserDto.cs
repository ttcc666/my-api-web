namespace MyApiWeb.Models.DTOs
{
    /// <summary>
    /// 在线用户数据传输对象
    /// </summary>
    public class OnlineUserDto
    {
        /// <summary>
        /// 记录 ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

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
        /// 连接建立时间
        /// </summary>
        public DateTimeOffset ConnectedAt { get; set; }

        /// <summary>
        /// 最后心跳时间
        /// </summary>
        public DateTimeOffset LastHeartbeatAt { get; set; }

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
        /// 在线状态
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// 在线时长 (秒)
        /// </summary>
        public long OnlineDurationSeconds { get; set; }
    }

    /// <summary>
    /// 在线用户统计数据传输对象
    /// </summary>
    public class OnlineUserStatisticsDto
    {
        /// <summary>
        /// 当前在线总人数
        /// </summary>
        public int TotalOnlineUsers { get; set; }

        /// <summary>
        /// 当前在线连接总数 (同一用户可能有多个连接)
        /// </summary>
        public int TotalConnections { get; set; }

        /// <summary>
        /// 今日峰值在线人数
        /// </summary>
        public int TodayPeakUsers { get; set; }

        /// <summary>
        /// 活跃用户数 (最近5分钟有心跳)
        /// </summary>
        public int ActiveUsers { get; set; }

        /// <summary>
        /// 空闲用户数 (超过5分钟无心跳但未超时)
        /// </summary>
        public int IdleUsers { get; set; }

        /// <summary>
        /// 平均在线时长 (秒)
        /// </summary>
        public long AverageOnlineDurationSeconds { get; set; }

        /// <summary>
        /// 统计时间
        /// </summary>
        public DateTimeOffset StatisticsTime { get; set; } = DateTimeOffset.Now;
    }

    /// <summary>
    /// 分页在线用户列表响应
    /// </summary>
    public class PagedOnlineUsersDto
    {
        /// <summary>
        /// 在线用户列表
        /// </summary>
        public List<OnlineUserDto> Users { get; set; } = new();

        /// <summary>
        /// 总记录数
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 当前页码
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// 每页大小
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
