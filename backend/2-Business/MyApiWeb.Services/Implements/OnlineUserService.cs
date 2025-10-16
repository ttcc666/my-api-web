using Microsoft.Extensions.Logging;
using MyApiWeb.Models.DTOs;
using MyApiWeb.Models.Entities;
using MyApiWeb.Repository;
using MyApiWeb.Services.Interfaces;
using SqlSugar;

namespace MyApiWeb.Services.Implements
{
    /// <summary>
    /// 在线用户服务实现
    /// </summary>
    public class OnlineUserService : IOnlineUserService
    {
        private readonly SqlSugarDbContext _dbContext;
        private readonly ILogger<OnlineUserService> _logger;

        public OnlineUserService(
            SqlSugarDbContext dbContext,
            ILogger<OnlineUserService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// 记录用户上线
        /// </summary>
        public async Task<OnlineUser> RecordUserConnectedAsync(
            string connectionId,
            string userId,
            string? username,
            string? ipAddress,
            string? userAgent,
            string? room)
        {
            var onlineUser = new OnlineUser
            {
                ConnectionId = connectionId,
                UserId = userId,
                Username = username,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Room = room,
                ConnectedAt = DateTimeOffset.Now,
                LastHeartbeatAt = DateTimeOffset.Now,
                Status = "Online"
            };

            await _dbContext.Db.Insertable(onlineUser).ExecuteCommandAsync();

            _logger.LogInformation(
                "用户上线记录已创建: UserId={UserId}, ConnectionId={ConnectionId}, IP={IpAddress}",
                userId, connectionId, ipAddress);

            return onlineUser;
        }

        /// <summary>
        /// 记录用户下线
        /// </summary>
        public async Task<bool> RecordUserDisconnectedAsync(string connectionId)
        {
            var result = await _dbContext.Db.Updateable<OnlineUser>()
                .SetColumns(u => new OnlineUser
                {
                    Status = "Offline",
                    DisconnectedAt = DateTimeOffset.Now
                })
                .Where(u => u.ConnectionId == connectionId)
                .ExecuteCommandAsync();

            if (result > 0)
            {
                _logger.LogInformation("用户下线记录已更新: ConnectionId={ConnectionId}", connectionId);
            }

            return result > 0;
        }

        /// <summary>
        /// 更新用户心跳时间
        /// </summary>
        public async Task<bool> UpdateHeartbeatAsync(string connectionId)
        {
            var result = await _dbContext.Db.Updateable<OnlineUser>()
                .SetColumns(u => new OnlineUser
                {
                    LastHeartbeatAt = DateTimeOffset.Now,
                    Status = "Online"
                })
                .Where(u => u.ConnectionId == connectionId)
                .ExecuteCommandAsync();

            return result > 0;
        }

        /// <summary>
        /// 分页查询在线用户列表
        /// </summary>
        public async Task<PagedOnlineUsersDto> GetOnlineUsersAsync(
            int pageNumber = 1,
            int pageSize = 20,
            string? status = null,
            string? userId = null,
            string? room = null)
        {
            var query = _dbContext.Db.Queryable<OnlineUser>();

            // 应用筛选条件
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(u => u.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(userId))
            {
                query = query.Where(u => u.UserId == userId);
            }

            if (!string.IsNullOrWhiteSpace(room))
            {
                query = query.Where(u => u.Room == room);
            }

            // 获取总数
            var totalCount = await query.CountAsync();

            // 分页查询
            var onlineUsers = await query
                .OrderByDescending(u => u.LastHeartbeatAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 转换为 DTO
            var userDtos = onlineUsers.Select(u => new OnlineUserDto
            {
                Id = u.Id,
                ConnectionId = u.ConnectionId,
                UserId = u.UserId,
                Username = u.Username,
                ConnectedAt = u.ConnectedAt,
                LastHeartbeatAt = u.LastHeartbeatAt,
                IpAddress = u.IpAddress,
                UserAgent = u.UserAgent,
                Room = u.Room,
                Status = u.Status,
                OnlineDurationSeconds = (long)(DateTimeOffset.Now - u.ConnectedAt).TotalSeconds
            }).ToList();

            return new PagedOnlineUsersDto
            {
                Users = userDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// 获取在线用户统计信息
        /// </summary>
        public async Task<OnlineUserStatisticsDto> GetStatisticsAsync()
        {
            var now = DateTimeOffset.Now;
            var fiveMinutesAgo = now.AddMinutes(-5);
            var todayStart = now.Date;

            // 当前在线用户 (状态为 Online 的去重用户数)
            var totalOnlineUsers = await _dbContext.Db.Queryable<OnlineUser>()
                .Where(u => u.Status == "Online")
                .Select(u => u.UserId)
                .Distinct()
                .CountAsync();

            // 当前在线连接总数
            var totalConnections = await _dbContext.Db.Queryable<OnlineUser>()
                .Where(u => u.Status == "Online")
                .CountAsync();

            // 活跃用户数 (最近5分钟有心跳)
            var activeUsers = await _dbContext.Db.Queryable<OnlineUser>()
                .Where(u => u.Status == "Online" && u.LastHeartbeatAt >= fiveMinutesAgo)
                .Select(u => u.UserId)
                .Distinct()
                .CountAsync();

            // 空闲用户数
            var idleUsers = await _dbContext.Db.Queryable<OnlineUser>()
                .Where(u => u.Status == "Online" && u.LastHeartbeatAt < fiveMinutesAgo)
                .Select(u => u.UserId)
                .Distinct()
                .CountAsync();

            // 今日峰值在线人数 (简化实现:当前在线数)
            var todayPeakUsers = totalOnlineUsers;

            // 平均在线时长 (简化实现:查询所有在线用户并计算平均时长)
            var onlineUsers = await _dbContext.Db.Queryable<OnlineUser>()
                .Where(u => u.Status == "Online")
                .Select(u => new { u.ConnectedAt })
                .ToListAsync();

            var averageDuration = onlineUsers.Any()
                ? (long)onlineUsers.Average(u => (now - u.ConnectedAt).TotalSeconds)
                : 0;

            return new OnlineUserStatisticsDto
            {
                TotalOnlineUsers = totalOnlineUsers,
                TotalConnections = totalConnections,
                TodayPeakUsers = todayPeakUsers,
                ActiveUsers = activeUsers,
                IdleUsers = idleUsers,
                AverageOnlineDurationSeconds = averageDuration,
                StatisticsTime = now
            };
        }

        /// <summary>
        /// 根据用户ID查询该用户的所有连接
        /// </summary>
        public async Task<List<OnlineUserDto>> GetUserConnectionsAsync(string userId)
        {
            var connections = await _dbContext.Db.Queryable<OnlineUser>()
                .Where(u => u.UserId == userId && u.Status == "Online")
                .OrderByDescending(u => u.LastHeartbeatAt)
                .ToListAsync();

            return connections.Select(u => new OnlineUserDto
            {
                Id = u.Id,
                ConnectionId = u.ConnectionId,
                UserId = u.UserId,
                Username = u.Username,
                ConnectedAt = u.ConnectedAt,
                LastHeartbeatAt = u.LastHeartbeatAt,
                IpAddress = u.IpAddress,
                UserAgent = u.UserAgent,
                Room = u.Room,
                Status = u.Status,
                OnlineDurationSeconds = (long)(DateTimeOffset.Now - u.ConnectedAt).TotalSeconds
            }).ToList();
        }

        /// <summary>
        /// 根据连接ID查询在线用户
        /// </summary>
        public async Task<OnlineUser?> GetByConnectionIdAsync(string connectionId)
        {
            return await _dbContext.Db.Queryable<OnlineUser>()
                .Where(u => u.ConnectionId == connectionId)
                .FirstAsync();
        }

        /// <summary>
        /// 强制下线指定连接
        /// </summary>
        public async Task<bool> ForceDisconnectAsync(string connectionId)
        {
            var result = await _dbContext.Db.Updateable<OnlineUser>()
                .SetColumns(u => new OnlineUser
                {
                    Status = "Offline",
                    DisconnectedAt = DateTimeOffset.Now
                })
                .Where(u => u.ConnectionId == connectionId && u.Status == "Online")
                .ExecuteCommandAsync();

            if (result > 0)
            {
                _logger.LogWarning("连接已被强制下线: ConnectionId={ConnectionId}", connectionId);
            }

            return result > 0;
        }

        /// <summary>
        /// 清理超时未心跳的连接记录
        /// </summary>
        public async Task<int> CleanupTimeoutConnectionsAsync(int timeoutMinutes = 15)
        {
            var timeoutThreshold = DateTimeOffset.Now.AddMinutes(-timeoutMinutes);

            var result = await _dbContext.Db.Updateable<OnlineUser>()
                .SetColumns(u => new OnlineUser
                {
                    Status = "Offline",
                    DisconnectedAt = DateTimeOffset.Now
                })
                .Where(u => u.Status == "Online" && u.LastHeartbeatAt < timeoutThreshold)
                .ExecuteCommandAsync();

            if (result > 0)
            {
                _logger.LogInformation("已清理 {Count} 个超时连接", result);
            }

            return result;
        }

        /// <summary>
        /// 获取当前在线用户数量
        /// </summary>
        public async Task<int> GetOnlineUserCountAsync()
        {
            return await _dbContext.Db.Queryable<OnlineUser>()
                .Where(u => u.Status == "Online")
                .Select(u => u.UserId)
                .Distinct()
                .CountAsync();
        }
    }
}
