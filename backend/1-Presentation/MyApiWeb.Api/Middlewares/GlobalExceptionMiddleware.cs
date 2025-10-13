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
            _logger.LogError(exception, "An unhandled exception has occurred.");

            context.Response.ContentType = "application/json";
            ApiResponse response;

            switch (exception)
            {
                case BusinessException ex:
                    context.Response.StatusCode = ex.Code;
                    response = new ApiResponse(false, ex.Code, ex.Message);
                    break;
                default:
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    response = new ApiResponse(false, StatusCodes.Status500InternalServerError, "服务器内部错误，请联系管理员。");
                    break;
            }

            var json = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(json);
        }
    }
}