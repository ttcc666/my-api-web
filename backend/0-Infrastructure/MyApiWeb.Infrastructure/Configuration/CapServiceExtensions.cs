using DotNetCore.CAP;
using Savorboard.CAP.InMemoryMessageQueue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MyApiWeb.Infrastructure.Configuration;

/// <summary>
/// CAP 消息总线服务配置扩展
/// 支持多种存储和传输方式的灵活配置
/// </summary>
public static class CapServiceExtensions
{
    /// <summary>
    /// 添加CAP消息总线配置
    /// </summary>
    public static IServiceCollection AddCapMessageBus(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddCap(options =>
        {
            ConfigureStorage(options, configuration);
            ConfigureTransport(options, configuration);

            options.FailedRetryCount = configuration.GetValue<int>("CAP:FailedRetryCount", 3);
            options.FailedRetryInterval = configuration.GetValue<int>("CAP:FailedRetryInterval", 60);
            options.ConsumerThreadCount = configuration.GetValue<int>("CAP:ConsumerThreadCount", 1);
            
            // 配置 Dashboard
            options.UseDashboard();

            // 配置失败回调
            options.FailedThresholdCallback = failed =>
            {
                var logger = failed.ServiceProvider.GetRequiredService<ILogger<CapOptions>>();
                logger.LogError(
                    "CAP消息处理失败达到阈值: MessageType={MessageType}, Message={Message}",
                    failed.MessageType,
                    failed.Message.ToString()
                );
            };
        });

        return services;
    }

    /// <summary>
    /// 配置消息存储
    /// </summary>
    private static void ConfigureStorage(CapOptions options, IConfiguration configuration)
    {
        var storageType = configuration["CAP:StorageType"] ?? "InMemory";

        switch (storageType.ToLower())
        {
            case "sqlserver":
                options.UseSqlServer(sqlOptions =>
                {
                    sqlOptions.ConnectionString = configuration.GetConnectionString("DefaultConnection")!;
                    sqlOptions.Schema = configuration["CAP:Storage:Schema"] ?? "cap";
                });
                break;

            case "mysql":
                options.UseMySql(mysqlOptions =>
                {
                    mysqlOptions.ConnectionString = configuration.GetConnectionString("DefaultConnection")!;
                    mysqlOptions.TableNamePrefix = configuration["CAP:Storage:TablePrefix"] ?? "cap";
                });
                break;

            case "postgresql":
                options.UsePostgreSql(pgOptions =>
                {
                    pgOptions.ConnectionString = configuration.GetConnectionString("DefaultConnection")!;
                    pgOptions.Schema = configuration["CAP:Storage:Schema"] ?? "cap";
                });
                break;

            case "mongodb":
                options.UseMongoDB(mongoOptions =>
                {
                    mongoOptions.DatabaseConnection = configuration.GetConnectionString("DefaultConnection")!;
                    mongoOptions.DatabaseName = configuration["CAP:Storage:DatabaseName"] ?? "cap";
                    mongoOptions.PublishedCollection = configuration["CAP:Storage:PublishedCollection"] ?? "published";
                    mongoOptions.ReceivedCollection = configuration["CAP:Storage:ReceivedCollection"] ?? "received";
                });
                break;

            case "inmemory":
                options.UseInMemoryStorage();
                break;

            default:
                throw new ArgumentException($"不支持的存储类型: {storageType}");
        }
    }

    /// <summary>
    /// 配置消息传输
    /// </summary>
    private static void ConfigureTransport(CapOptions options, IConfiguration configuration)
    {
        var transportType = configuration["CAP:TransportType"] ?? "InMemory";

        switch (transportType.ToLower())
        {
            case "inmemory":
                options.UseInMemoryMessageQueue();
                break;
                
            case "rabbitmq":
                options.UseRabbitMQ(rabbitOptions =>
                {
                    rabbitOptions.HostName = configuration["CAP:Transport:HostName"] ?? "localhost";
                    rabbitOptions.Port = configuration.GetValue<int>("CAP:Transport:Port", 5672);
                    rabbitOptions.UserName = configuration["CAP:Transport:UserName"] ?? "guest";
                    rabbitOptions.Password = configuration["CAP:Transport:Password"] ?? "guest";
                    rabbitOptions.VirtualHost = configuration["CAP:Transport:VirtualHost"] ?? "/";
                    rabbitOptions.ExchangeName = configuration["CAP:Transport:ExchangeName"] ?? "cap.default.router";
                });
                break;

            case "kafka":
                options.UseKafka(kafkaOptions =>
                {
                    kafkaOptions.Servers = configuration["CAP:Transport:Servers"] ?? "localhost:9092";
                    kafkaOptions.ConnectionPoolSize = configuration.GetValue<int>("CAP:Transport:ConnectionPoolSize", 10);
                });
                break;

            case "azureservicebus":
                options.UseAzureServiceBus(asbOptions =>
                {
                    asbOptions.ConnectionString = configuration["CAP:Transport:ConnectionString"] 
                        ?? throw new ArgumentException("Azure Service Bus连接字符串未配置");
                    asbOptions.TopicPath = configuration["CAP:Transport:TopicPath"] ?? "cap";
                });
                break;

            case "nats":
                options.UseNATS(natsOptions =>
                {
                    natsOptions.Servers = configuration["CAP:Transport:Servers"] ?? "nats://localhost:4222";
                    natsOptions.ConnectionPoolSize = configuration.GetValue<int>("CAP:Transport:ConnectionPoolSize", 10);
                });
                break;

            case "redis":
                options.UseRedis(redisOptions =>
                {
                    var redisConfig = configuration["CAP:Transport:Configuration"] ?? "localhost";
                    redisOptions.Configuration = StackExchange.Redis.ConfigurationOptions.Parse(redisConfig);
                    redisOptions.StreamEntriesCount = (uint)configuration.GetValue<int>("CAP:Transport:StreamEntriesCount", 10);
                });
                break;

            default:
                throw new ArgumentException($"不支持的传输类型: {transportType}");
        }
    }
}