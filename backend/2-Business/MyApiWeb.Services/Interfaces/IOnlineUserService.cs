using MyApiWeb.Models.DTOs;
using MyApiWeb.Models.Entities;

namespace MyApiWeb.Services.Interfaces
{
    /// <summary>
    /// 在线用户服务接口
    /// 管理 SignalR 在线用户的连接、状态和查询
    /// </summary>
    public interface IOnlineUserService
    {
        /// <summary>
        /// 记录用户上线
        /// </summary>
        /// <param name="connectionId">连接ID</param>
        /// <param name="userId">用户ID</param>
        /// <param name="username">用户名</param>
        /// <param name="ipAddress">IP地址</param>
        /// <param name="userAgent">浏览器信息</param>
        /// <param name="room">房间名称</param>
        /// <returns>在线用户记录</returns>
        Task<OnlineUser> RecordUserConnectedAsync(
            string connectionId,
            string userId,
            string? username,
            string? ipAddress,
            string? userAgent,
            string? room);

        /// <summary>
        /// 记录用户下线
        /// </summary>
        /// <param name="connectionId">连接ID</param>
        /// <returns>操作是否成功</returns>
        Task<bool> RecordUserDisconnectedAsync(string connectionId);

        /// <summary>
        /// 更新用户心跳时间
        /// </summary>
        /// <param name="connectionId">连接ID</param>
        /// <returns>操作是否成功</returns>
        Task<bool> UpdateHeartbeatAsync(string connectionId);

        /// <summary>
        /// 分页查询在线用户列表
        /// </summary>
        /// <param name="pageNumber">页码</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="status">状态筛选 (可选)</param>
        /// <param name="userId">用户ID筛选 (可选)</param>
        /// <param name="room">房间筛选 (可选)</param>
        /// <returns>分页在线用户列表</returns>
        Task<PagedOnlineUsersDto> GetOnlineUsersAsync(
            int pageNumber = 1,
            int pageSize = 20,
            string? status = null,
            string? userId = null,
            string? room = null);

        /// <summary>
        /// 获取在线用户统计信息
        /// </summary>
        /// <returns>在线用户统计数据</returns>
        Task<OnlineUserStatisticsDto> GetStatisticsAsync();

        /// <summary>
        /// 根据用户ID查询该用户的所有连接
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>在线用户连接列表</returns>
        Task<List<OnlineUserDto>> GetUserConnectionsAsync(string userId);

        /// <summary>
        /// 根据连接ID查询在线用户
        /// </summary>
        /// <param name="connectionId">连接ID</param>
        /// <returns>在线用户信息,不存在则返回 null</returns>
        Task<OnlineUser?> GetByConnectionIdAsync(string connectionId);

        /// <summary>
        /// 强制下线指定连接
        /// </summary>
        /// <param name="connectionId">连接ID</param>
        /// <returns>操作是否成功</returns>
        Task<bool> ForceDisconnectAsync(string connectionId);

        /// <summary>
        /// 清理超时未心跳的连接记录
        /// </summary>
        /// <param name="timeoutMinutes">超时分钟数</param>
        /// <returns>清理的记录数量</returns>
        Task<int> CleanupTimeoutConnectionsAsync(int timeoutMinutes = 15);

        /// <summary>
        /// 获取当前在线用户数量
        /// </summary>
        /// <returns>在线用户数量</returns>
        Task<int> GetOnlineUserCountAsync();
    }
}
