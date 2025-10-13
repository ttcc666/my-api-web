using System.Text.Json;
using MyApiWeb.Models.DTOs;
using MyApiWeb.Models.Exceptions;

namespace MyApiWeb.Api.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            ApiResponse response;

            switch (exception)
            {
                case DomainException domainEx:
                    // 领域异常(自定义异常)
                    context.Response.StatusCode = domainEx.StatusCode;
                    response = new ApiResponse(
                        false,
                        domainEx.StatusCode,
                        domainEx.Message
                    );

                    // 根据状态码级别记录日志
                    if (domainEx.StatusCode >= 500)
                    {
                        _logger.LogError(domainEx,
                            "领域异常: {ErrorCode} - {Message}",
                            domainEx.ErrorCode,
                            domainEx.Message);
                    }
                    else
                    {
                        _logger.LogWarning(domainEx,
                            "业务异常: {ErrorCode} - {Message}",
                            domainEx.ErrorCode,
                            domainEx.Message);
                    }
                    break;

                case BusinessException businessEx:
                    // 向后兼容旧的 BusinessException
                    context.Response.StatusCode = businessEx.Code;
                    response = new ApiResponse(false, businessEx.Code, businessEx.Message);
                    _logger.LogWarning(businessEx, "业务异常: {Message}", businessEx.Message);
                    break;

                default:
                    // 未知异常
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    response = new ApiResponse(
                        false,
                        StatusCodes.Status500InternalServerError,
                        "服务器内部错误,请联系管理员。"
                    );
                    _logger.LogError(exception, "未处理的异常: {ExceptionType} - {Message}",
                        exception.GetType().Name,
                        exception.Message);
                    break;
            }

            var json = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(json);
        }
    }
}