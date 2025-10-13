namespace MyApiWeb.Models.Exceptions;

/// <summary>
/// 资源未找到异常
/// </summary>
public class NotFoundException : DomainException
{
    public NotFoundException(
        string message = "请求的资源不存在",
        object? details = null,
        Exception? innerException = null)
        : base(
            message,
            DomainException.StatusCodes.Status404NotFound,
            "NOT_FOUND",
            details,
            innerException)
    {
    }
}