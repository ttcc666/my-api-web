using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApiWeb.Models.DTOs;

namespace MyApiWeb.Api.Helpers
{
    public static class ApiResultHelper
    {
        public static IActionResult Success<T>(T data, string message = "操作成功", int code = StatusCodes.Status200OK)
        {
            return new OkObjectResult(new ApiResponse<T>(true, code, message, data));
        }

        public static IActionResult SuccessMessage(string message = "操作成功", int code = StatusCodes.Status200OK)
        {
            return Success<object?>(null, message, code);
        }

        public static IActionResult Error(string message, int code = StatusCodes.Status500InternalServerError, object? data = null)
        {
            return new OkObjectResult(new ApiResponse<object?>(false, code, message, data));
        }
    }
}
