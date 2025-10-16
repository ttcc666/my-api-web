using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyApiWeb.Infrastructure.Jobs;
using Quartz;

namespace MyApiWeb.Infrastructure.Configuration
{
    /// <summary>
    /// Quartz.NET 调度服务配置扩展
    /// </summary>
    public static class QuartzServiceExtensions
    {
        /// <summary>
        /// 添加 Quartz.NET 调度服务并注册任务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configuration">配置</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddQuartzWithJobs(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // 配置 Quartz
            services.AddQuartz(q =>
            {
                // 使用唯一实例 ID (用于集群)
                q.SchedulerId = "MyApiWeb-Scheduler";

                // 配置在线用户清理任务
                ConfigureOnlineUserCleanupJob(q, configuration);
            });

            // 添加 Quartz 托管服务
            services.AddQuartzHostedService(options =>
            {
                // 等待任务完成后再关闭
                options.WaitForJobsToComplete = true;

                // 启动延迟 (秒)
                options.StartDelay = TimeSpan.FromSeconds(5);
            });

            return services;
        }

        /// <summary>
        /// 配置在线用户清理任务
        /// </summary>
        private static void ConfigureOnlineUserCleanupJob(
            IServiceCollectionQuartzConfigurator configurator,
            IConfiguration configuration)
        {
            // 定义 Job 键
            var jobKey = new JobKey("OnlineUserCleanupJob", "OnlineUserManagement");

            // 添加 Job
            configurator.AddJob<OnlineUserCleanupJob>(opts =>
            {
                opts.WithIdentity(jobKey)
                    .WithDescription("定期清理超时未心跳的在线用户连接")
                    .StoreDurably(); // 即使没有触发器也保留 Job
            });

            // 从配置读取 Cron 表达式
            var cronExpression = configuration["OnlineUser:CleanupCronExpression"] ?? "0 */5 * * * ?";

            // 添加触发器
            configurator.AddTrigger(opts =>
            {
                opts.ForJob(jobKey)
                    .WithIdentity("OnlineUserCleanupTrigger", "OnlineUserManagement")
                    .WithDescription($"触发在线用户清理任务 (Cron: {cronExpression})")
                    .WithCronSchedule(cronExpression, x =>
                    {
                        // Cron 表达式无效时的处理
                        x.WithMisfireHandlingInstructionFireAndProceed();
                    })
                    .StartNow(); // 立即启动
            });
        }
    }
}
