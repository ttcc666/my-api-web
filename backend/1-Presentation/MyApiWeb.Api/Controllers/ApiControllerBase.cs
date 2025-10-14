using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MyApiWeb.Api.Helpers;
using MyApiWeb.Services.Interfaces;

namespace MyApiWeb.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public abstract class ApiControllerBase<TService, TEntity, TKey> : ControllerBase
        where TKey : IEquatable<TKey>
    {
        protected readonly ILogger<ApiControllerBase<TService, TEntity, TKey>> _logger;
        protected readonly TService _service;

        protected ApiControllerBase(ILogger<ApiControllerBase<TService, TEntity, TKey>> logger, TService service)
        {
            _logger = logger;
            _service = service;
        }

        protected string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        protected IActionResult Success<T>(T data, string message = "操作成功", int code = StatusCodes.Status200OK)
        {
            return ApiResultHelper.Success(data, message, code);
        }

        protected IActionResult SuccessMessage(string message = "操作成功", int code = StatusCodes.Status200OK)
        {
            return ApiResultHelper.SuccessMessage(message, code);
        }

        protected IActionResult Error(string message, int code = StatusCodes.Status500InternalServerError, object? data = null)
        {
            return ApiResultHelper.Error(message, code, data);
        }

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
