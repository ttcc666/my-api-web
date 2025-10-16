using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MyApiWeb.Models.DTOs;
using MyApiWeb.Services.Interfaces;

namespace MyApiWeb.Api.Controllers
{
    /// <summary>
    /// 在线用户管理控制器
    /// 提供在线用户的查询、统计和管理功能
    /// </summary>
    [ApiController]
    [Route("api/online-users")]
    [Authorize]
    public class OnlineUsersController : ApiControllerBase
    {
        private readonly IOnlineUserService _onlineUserService;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ILogger<OnlineUsersController> _logger;

        public OnlineUsersController(
            IOnlineUserService onlineUserService,
            IHubContext<ChatHub> hubContext,
            ILogger<OnlineUsersController> logger)
        {
            _onlineUserService = onlineUserService;
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <summary>
        /// 分页查询在线用户列表
        /// </summary>
        /// <param name="pageNumber">页码 (默认: 1)</param>
        /// <param name="pageSize">每页大小 (默认: 20)</param>
        /// <param name="status">状态筛选 (可选: Online, Idle, Offline)</param>
        /// <param name="userId">用户ID筛选 (可选)</param>
        /// <param name="room">房间筛选 (可选)</param>
        /// <returns>分页在线用户列表</returns>
        [HttpGet]
        public async Task<IActionResult> GetOnlineUsers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? status = null,
            [FromQuery] string? userId = null,
            [FromQuery] string? room = null)
        {
            try
            {
                var result = await _onlineUserService.GetOnlineUsersAsync(
                    pageNumber, pageSize, status, userId, room);

                return Success(result, "查询在线用户列表成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询在线用户列表失败");
                return Error("查询在线用户列表失败");
            }
        }

        /// <summary>
        /// 获取在线用户统计信息
        /// </summary>
        /// <returns>在线用户统计数据</returns>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var statistics = await _onlineUserService.GetStatisticsAsync();
                return Success(statistics, "获取统计信息成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取统计信息失败");
                return Error("获取统计信息失败");
            }
        }

        /// <summary>
        /// 查询指定用户的所有连接
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>该用户的所有在线连接</returns>
        [HttpGet("by-user/{userId}")]
        public async Task<IActionResult> GetUserConnections(string userId)
        {
            try
            {
                var connections = await _onlineUserService.GetUserConnectionsAsync(userId);
                return Success(connections, $"查询用户 {userId} 的连接成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询用户连接失败: UserId={UserId}", userId);
                return Error("查询用户连接失败");
            }
        }

        /// <summary>
        /// 获取当前在线用户数量
        /// </summary>
        /// <returns>在线用户数量</returns>
        [HttpGet("count")]
        public async Task<IActionResult> GetOnlineUserCount()
        {
            try
            {
                var count = await _onlineUserService.GetOnlineUserCountAsync();
                return Success(new { count }, "获取在线用户数量成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取在线用户数量失败");
                return Error("获取在线用户数量失败");
            }
        }

        /// <summary>
        /// 强制下线指定连接
        /// </summary>
        /// <param name="connectionId">连接ID</param>
        /// <param name="reason">下线原因(可选)</param>
        /// <returns>操作结果</returns>
        [HttpDelete("{connectionId}")]
        public async Task<IActionResult> ForceDisconnect(string connectionId, [FromQuery] string? reason = null)
        {
            try
            {
                // 获取连接信息
                var onlineUser = await _onlineUserService.GetByConnectionIdAsync(connectionId);
                if (onlineUser == null)
                {
                    return Error("连接不存在或已下线");
                }

                // 获取当前用户ID
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                // 检查是否尝试踢自己下线
                if (!string.IsNullOrEmpty(currentUserId) && currentUserId == onlineUser.UserId)
                {
                    _logger.LogWarning(
                        "用户尝试踢自己下线: UserId={UserId}, ConnectionId={ConnectionId}",
                        currentUserId, connectionId);
                    return Error("不能踢自己下线");
                }

                // 更新数据库状态
                var success = await _onlineUserService.ForceDisconnectAsync(connectionId);

                if (success)
                {
                    // 通知客户端强制断开
                    await _hubContext.Clients.Client(connectionId).SendAsync("forceDisconnect", new
                    {
                        reason = reason ?? "管理员强制下线",
                        timestamp = DateTimeOffset.Now
                    });

                    _logger.LogWarning(
                        "管理员强制下线用户: Operator={Operator}, ConnectionId={ConnectionId}, UserId={UserId}, Reason={Reason}",
                        currentUserId, connectionId, onlineUser.UserId, reason ?? "无");

                    return Success<object?>(null, "强制下线成功");
                }

                return Error("强制下线失败");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "强制下线失败: ConnectionId={ConnectionId}", connectionId);
                return Error("强制下线失败");
            }
        }

        /// <summary>
        /// 手动触发清理超时连接
        /// </summary>
        /// <param name="timeoutMinutes">超时分钟数 (默认: 15)</param>
        /// <returns>清理的记录数量</returns>
        [HttpPost("cleanup")]
        public async Task<IActionResult> CleanupTimeoutConnections([FromQuery] int timeoutMinutes = 15)
        {
            try
            {
                var count = await _onlineUserService.CleanupTimeoutConnectionsAsync(timeoutMinutes);
                return Success(new { cleanedCount = count }, $"清理完成,共清理 {count} 个超时连接");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理超时连接失败");
                return Error("清理超时连接失败");
            }
        }
    }
}
