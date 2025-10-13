namespace MyApiWeb.Models.Exceptions;

/// <summary>
/// 冲突异常(资源已存在或状态冲突)
/// </summary>
public class ConflictException : DomainException
{
    public ConflictException(
        string message = "资源已存在或状态冲突",
        object? details = null,
        Exception? innerException = null)
        : base(
            message,
            DomainException.StatusCodes.Status409Conflict,
            "CONFLICT",
            details,
            innerException)
    {
    }
}