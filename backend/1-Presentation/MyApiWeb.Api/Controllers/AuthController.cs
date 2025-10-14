using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApiWeb.Models.DTOs;
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
        [ProducesResponseType(typeof(ApiResponse<UserPermissionInfoDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyPermissions()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Error("用户未认证", StatusCodes.Status401Unauthorized);
            }

            var userPermissions = await _service.GetUserPermissionsAsync(userId);
            return Success(userPermissions);
        }

        /// <summary>
        /// 检查当前用户是否拥有指定权限
        /// </summary>
        /// <param name="permission">权限名称</param>
        /// <returns>权限检查结果</returns>
        [HttpGet("me/permissions/check")]
        [ProducesResponseType(typeof(ApiResponse<UserPermissionCheckDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckMyPermission([FromQuery] string permission)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Error("用户未认证", StatusCodes.Status401Unauthorized);
            }

            var result = await _service.CheckUserPermissionAsync(userId, permission);
            return Success(result);
        }

        /// <summary>
        /// 获取当前用户信息（包含权限）
        /// </summary>
        /// <returns>用户信息</returns>
        [HttpGet("me")]
        [ProducesResponseType(typeof(ApiResponse<CurrentUserInfoDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var username = User.FindFirstValue(ClaimTypes.Name);
            
            if (string.IsNullOrEmpty(userId))
            {
                return Error("用户未认证", StatusCodes.Status401Unauthorized);
            }

            var userPermissions = await _service.GetUserPermissionsAsync(userId);
            
            var currentUser = new CurrentUserInfoDto
            {
                Id = userId,
                Username = username ?? string.Empty,
                Permissions = userPermissions.EffectivePermissions.Select(p => p.Name).ToList(),
                Roles = userPermissions.Roles.Select(r => r.Name).ToList()
            };

            return Success(currentUser);
        }
    }
}
