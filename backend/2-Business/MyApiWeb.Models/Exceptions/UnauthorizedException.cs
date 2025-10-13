namespace MyApiWeb.Models.Exceptions;

/// <summary>
/// 认证异常(用户未登录或令牌无效)
/// </summary>
public class UnauthorizedException : DomainException
{
    public UnauthorizedException(
        string message = "用户未认证或认证已过期",
        object? details = null,
        Exception? innerException = null)
        : base(
            message,
            DomainException.StatusCodes.Status401Unauthorized,
            "UNAUTHORIZED",
            details,
            innerException)
    {
    }
}