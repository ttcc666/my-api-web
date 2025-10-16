using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyApiWeb.Services.Interfaces.Hub;
using Quartz;

namespace MyApiWeb.Infrastructure.Jobs
{
    /// <summary>
    /// 在线用户清理定时任务
    /// 使用 Quartz.NET 调度,定期清理超时未心跳的连接记录
    /// </summary>
    [DisallowConcurrentExecution] // 防止任务并发执行
    public class OnlineUserCleanupJob : IJob
    {
        private readonly IOnlineUserService _onlineUserService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OnlineUserCleanupJob> _logger;

        public OnlineUserCleanupJob(
            IOnlineUserService onlineUserService,
            IConfiguration configuration,
            ILogger<OnlineUserCleanupJob> logger)
        {
            _onlineUserService = onlineUserService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var jobKey = context.JobDetail.Key;
            _logger.LogDebug("开始执行在线用户清理任务: {JobKey}", jobKey);

            try
            {
                // 从配置读取超时时间
                var connectionTimeoutMinutes = _configuration.GetValue<int>("OnlineUser:ConnectionTimeoutMinutes", 15);

                // 执行清理
                var cleanedCount = await _onlineUserService.CleanupTimeoutConnectionsAsync(connectionTimeoutMinutes);

                if (cleanedCount > 0)
                {
                    _logger.LogInformation(
                        "在线用户清理任务完成: {JobKey}, 清理了 {Count} 个超时连接",
                        jobKey, cleanedCount);
                }
                else
                {
                    _logger.LogDebug(
                        "在线用户清理任务完成: {JobKey}, 没有超时连接需要清理",
                        jobKey);
                }

                // 记录下次执行时间
                var nextFireTime = context.NextFireTimeUtc;
                if (nextFireTime.HasValue)
                {
                    _logger.LogDebug(
                        "下次清理任务执行时间: {NextFireTime}",
                        nextFireTime.Value.LocalDateTime);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行在线用户清理任务时发生错误: {JobKey}", jobKey);

                // 抛出异常以便 Quartz 可以记录失败并根据配置进行重试
                throw new JobExecutionException(ex, refireImmediately: false);
            }
        }
    }
}
