namespace MyApiWeb.Models.Exceptions;

/// <summary>
/// 领域异常基类,所有业务异常的基类
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>
    /// HTTP状态码
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// 错误代码(用于前端国际化或特定错误处理)
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// 额外的错误详情(可选)
    /// </summary>
    public object? Details { get; }

    protected DomainException(
        string message,
        int statusCode,
        string errorCode,
        object? details = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        Details = details;
    }

    /// <summary>
    /// HTTP状态码常量(避免引用Microsoft.AspNetCore.Http)
    /// </summary>
    public static class StatusCodes
    {
        public const int Status400BadRequest = 400;
        public const int Status401Unauthorized = 401;
        public const int Status403Forbidden = 403;
        public const int Status404NotFound = 404;
        public const int Status409Conflict = 409;
        public const int Status500InternalServerError = 500;
    }
}