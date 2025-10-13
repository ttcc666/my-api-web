using Microsoft.AspNetCore.Mvc;
using MyApiWeb.Models.DTOs;

namespace MyApiWeb.Api.Helpers
{
    public static class ApiResultHelper
    {
        public static IActionResult Success<T>(T data, string message = "操作成功")
        {
            return new OkObjectResult(new ApiResponse<T>(true, 200, message, data));
        }

        public static IActionResult Success(string message = "操作成功")
        {
            return new OkObjectResult(new ApiResponse(true, 200, message));
        }

        public static IActionResult Error(string message, int code = 500)
        {
            return new ObjectResult(new ApiResponse(false, code, message))
            {
                StatusCode = code
            };
        }
    }
}