namespace MyApiWeb.Models.Exceptions;

/// <summary>
/// 禁止访问异常(用户无权限访问资源)
/// </summary>
public class ForbiddenException : DomainException
{
    public ForbiddenException(
        string message = "用户无权限访问此资源",
        object? details = null,
        Exception? innerException = null)
        : base(
            message,
            DomainException.StatusCodes.Status403Forbidden,
            "FORBIDDEN",
            details,
            innerException)
    {
    }
}