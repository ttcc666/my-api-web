using DotNetCore.CAP;
using Microsoft.Extensions.Logging;

namespace MyApiWeb.Services.Implements;

/// <summary>
/// CAP失败消息订阅者
/// 用于记录所有失败的消息处理日志
/// </summary>
public class CapFailedMessageSubscriber : ICapSubscribe
{
    private readonly ILogger<CapFailedMessageSubscriber> _logger;

    public CapFailedMessageSubscriber(ILogger<CapFailedMessageSubscriber> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 订阅所有失败的消息
    /// CAP会自动将失败的消息发送到这个订阅者
    /// </summary>
    [CapSubscribe("cap.failed.message")]
    public Task HandleFailedMessage(CapFailedMessageEvent failedEvent)
    {
        _logger.LogError(
            "CAP消息处理失败 - Topic: {Topic}, MessageId: {MessageId}, RetryCount: {RetryCount}, ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}",
            failedEvent.Topic,
            failedEvent.MessageId,
            failedEvent.RetryCount,
            failedEvent.ErrorMessage,
            failedEvent.StackTrace
        );

        // 可以在这里添加额外的失败处理逻辑
        // 例如: 发送告警通知、记录到专门的错误日志表等

        return Task.CompletedTask;
    }
}

/// <summary>
/// CAP失败消息事件数据
/// </summary>
public class CapFailedMessageEvent
{
    public string Topic { get; set; } = string.Empty;
    public string MessageId { get; set; } = string.Empty;
    public int RetryCount { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public DateTime FailedTime { get; set; }
}