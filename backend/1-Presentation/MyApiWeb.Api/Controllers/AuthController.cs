using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApiWeb.Api.Helpers;
using MyApiWeb.Models.Entities;
using MyApiWeb.Services.Interfaces;
using System.Security.Claims;

namespace MyApiWeb.Api.Controllers
{
    /// <summary>
    /// 认证控制器
    /// </summary>
    [Authorize]
    public class AuthController : ApiControllerBase<IPermissionService, Permission, string>
    {
        public AuthController(ILogger<AuthController> logger, IPermissionService permissionService)
            : base(logger, permissionService)
        {
        }

        /// <summary>
        /// 获取当前用户的权限信息
        /// </summary>
        /// <returns>用户权限信息</returns>
        [HttpGet("me/permissions")]
        public async Task<IActionResult> GetMyPermissions()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResultHelper.Error("用户未认证", 401));
            }

            var userPermissions = await _service.GetUserPermissionsAsync(userId);
            return Ok(ApiResultHelper.Success(userPermissions));
        }

        /// <summary>
        /// 检查当前用户是否拥有指定权限
        /// </summary>
        /// <param name="permission">权限名称</param>
        /// <returns>权限检查结果</returns>
        [HttpGet("me/permissions/check")]
        public async Task<IActionResult> CheckMyPermission([FromQuery] string permission)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResultHelper.Error("用户未认证", 401));
            }

            var result = await _service.CheckUserPermissionAsync(userId, permission);
            return Ok(ApiResultHelper.Success(result));
        }

        /// <summary>
        /// 获取当前用户信息（包含权限）
        /// </summary>
        /// <returns>用户信息</returns>
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var username = User.FindFirstValue(ClaimTypes.Name);
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResultHelper.Error("用户未认证", 401));
            }

            var userPermissions = await _service.GetUserPermissionsAsync(userId);
            
            var currentUser = new
            {
                Id = userId,
                Username = username,
                Permissions = userPermissions.EffectivePermissions.Select(p => p.Name).ToList(),
                Roles = userPermissions.Roles.Select(r => r.Name).ToList()
            };

            return Ok(ApiResultHelper.Success(currentUser));
        }
    }
}