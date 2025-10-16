using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApiWeb.Models.DTOs;

namespace MyApiWeb.Infrastructure.Helpers
{
    /// <summary>
    /// API 响应结果辅助类
    /// </summary>
    public static class ApiResultHelper
    {
        /// <summary>
        /// 返回成功结果
        /// </summary>
        public static IActionResult Success<T>(T data, string message = "操作成功", int code = StatusCodes.Status200OK)
        {
            return new OkObjectResult(new ApiResponse<T>(true, code, message, data));
        }

        /// <summary>
        /// 返回成功消息（无数据）
        /// </summary>
        public static IActionResult SuccessMessage(string message = "操作成功", int code = StatusCodes.Status200OK)
        {
            return Success<object?>(null, message, code);
        }

        /// <summary>
        /// 返回错误结果
        /// </summary>
        public static IActionResult Error(string message, int code = StatusCodes.Status500InternalServerError, object? data = null)
        {
            return new OkObjectResult(new ApiResponse<object?>(false, code, message, data));
        }
    }
}