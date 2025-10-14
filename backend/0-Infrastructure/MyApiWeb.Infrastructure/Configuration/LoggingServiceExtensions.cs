using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace MyApiWeb.Infrastructure.Configuration
{
    /// <summary>
    /// Serilog 日志服务配置扩展
    /// </summary>
    public static class LoggingServiceExtensions
    {
        /// <summary>
        /// 配置 Serilog 日志服务
        /// </summary>
        public static IHostBuilder AddSerilogLogging(this IHostBuilder host)
        {
            // 配置 Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithEnvironmentName()
                .Enrich.WithMachineName()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    path: "logs/app-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    fileSizeLimitBytes: 10 * 1024 * 1024, // 10MB
                    rollOnFileSizeLimit: true,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] [{RequestId}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            // 使用 Serilog 替换默认日志提供程序
            host.UseSerilog();

            return host;
        }
    }
}
