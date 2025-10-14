using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MyApiWeb.Infrastructure.Helpers;
using MyApiWeb.Services.Interfaces;

namespace MyApiWeb.Api.Controllers
{
    /// <summary>
    /// API 控制器基类
    /// </summary>
    /// <typeparam name="TService">服务类型</typeparam>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <typeparam name="TKey">主键类型</typeparam>
    /// <remarks>
    /// 提供统一的 API 响应格式、错误处理和常用辅助方法。
    /// 所有业务 Controller 应继承此基类以保持一致的响应结构。
    /// 默认应用 [Authorize] 特性,要求 JWT 认证。
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public abstract class ApiControllerBase<TService, TEntity, TKey> : ControllerBase
        where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// 日志记录器实例
        /// </summary>
        protected readonly ILogger<ApiControllerBase<TService, TEntity, TKey>> _logger;

        /// <summary>
        /// 业务服务实例
        /// </summary>
        protected readonly TService _service;

        /// <summary>
        /// 初始化 API 控制器基类
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <param name="service">业务服务</param>
        protected ApiControllerBase(ILogger<ApiControllerBase<TService, TEntity, TKey>> logger, TService service)
        {
            _logger = logger;
            _service = service;
        }

        /// <summary>
        /// 获取当前登录用户的 ID
        /// </summary>
        /// <value>
        /// 从 JWT Token 的 ClaimTypes.NameIdentifier 中解析的用户 ID,如果未认证则返回 null
        /// </value>
        protected string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        /// <summary>
        /// 返回成功响应 (带数据)
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="data">响应数据</param>
        /// <param name="message">成功消息</param>
        /// <param name="code">HTTP 状态码</param>
        /// <returns>统一格式的成功响应</returns>
        protected IActionResult Success<T>(T data, string message = "操作成功", int code = StatusCodes.Status200OK)
        {
            return ApiResultHelper.Success(data, message, code);
        }

        /// <summary>
        /// 返回成功响应 (仅消息,无数据)
        /// </summary>
        /// <param name="message">成功消息</param>
        /// <param name="code">HTTP 状态码</param>
        /// <returns>统一格式的成功响应</returns>
        protected IActionResult SuccessMessage(string message = "操作成功", int code = StatusCodes.Status200OK)
        {
            return ApiResultHelper.SuccessMessage(message, code);
        }

        /// <summary>
        /// 返回错误响应
        /// </summary>
        /// <param name="message">错误消息</param>
        /// <param name="code">HTTP 状态码</param>
        /// <param name="data">附加错误数据 (可选)</param>
        /// <returns>统一格式的错误响应</returns>
        protected IActionResult Error(string message, int code = StatusCodes.Status500InternalServerError, object? data = null)
        {
            return ApiResultHelper.Error(message, code, data);
        }

        /// <summary>
        /// 返回模型验证错误响应
        /// </summary>
        /// <param name="modelState">模型状态字典</param>
        /// <param name="message">错误消息</param>
        /// <returns>包含详细验证错误信息的统一格式响应</returns>
        /// <remarks>
        /// 将 ModelState 中的验证错误转换为字典格式:
        /// { "字段名": ["错误消息1", "错误消息2"] }
        /// </remarks>
        protected IActionResult ValidationError(ModelStateDictionary modelState, string message = "请求参数无效")
        {
            var errors = modelState
                .Where(kvp => kvp.Value is { Errors.Count: > 0 })
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(error => error.ErrorMessage).ToArray());

            return Error(message, StatusCodes.Status400BadRequest, errors);
        }
    }
}
