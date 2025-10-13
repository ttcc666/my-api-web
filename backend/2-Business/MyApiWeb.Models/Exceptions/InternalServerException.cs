namespace MyApiWeb.Models.Exceptions;

/// <summary>
/// 内部服务器异常(系统错误)
/// </summary>
public class InternalServerException : DomainException
{
    public InternalServerException(
        string message = "服务器内部错误",
        object? details = null,
        Exception? innerException = null)
        : base(
            message,
            DomainException.StatusCodes.Status500InternalServerError,
            "INTERNAL_SERVER_ERROR",
            details,
            innerException)
    {
    }
}