namespace MyApiWeb.Models.Exceptions;

/// <summary>
/// 数据验证异常(输入数据不符合要求)
/// </summary>
public class ValidationException : DomainException
{
    public ValidationException(
        string message = "输入数据验证失败",
        object? details = null,
        Exception? innerException = null)
        : base(
            message,
            DomainException.StatusCodes.Status400BadRequest,
            "VALIDATION_ERROR",
            details,
            innerException)
    {
    }
}