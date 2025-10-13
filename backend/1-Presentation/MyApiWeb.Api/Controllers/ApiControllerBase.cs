using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        protected IActionResult Success<T>(T data, string message = "操作成功")
        {
            return ApiResultHelper.Success(data, message);
        }

        protected IActionResult Success(string message = "操作成功")
        {
            return ApiResultHelper.Success(message);
        }

        protected IActionResult Error(string message, int code = 500)
        {
            return ApiResultHelper.Error(message, code);
        }
    }
}