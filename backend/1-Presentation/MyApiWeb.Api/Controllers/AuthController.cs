using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApiWeb.Models.DTOs;
using MyApiWeb.Models.Entities.System;
using MyApiWeb.Services.Interfaces.System;
using System.Security.Claims;

namespace MyApiWeb.Api.Controllers
{
    /// <summary>
    /// 认证控制器
    /// </summary>
    /// <remarks>
    /// 提供当前登录用户的认证信息查询功能，包括权限检查和用户信息获取。
    /// 所有接口都需要用户已登录（JWT认证）。
    /// </remarks>
    [Authorize]
    public class AuthController : ApiControllerBase<IPermissionService, Permission, string>
    {
        private readonly IMenuService _menuService;

        /// <summary>
        /// 初始化认证控制器
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <param name="permissionService">权限服务</param>
        /// <param name="menuService">菜单服务</param>
        public AuthController(ILogger<AuthController> logger, IPermissionService permissionService, IMenuService menuService)
            : base(logger, permissionService)
        {
            _menuService = menuService;
        }

        /// <summary>
        /// 获取当前用户的权限信息
        /// </summary>
        /// <returns>用户权限信息</returns>
        /// <response code="200">成功返回用户权限信息，包括角色权限和直接权限</response>
        /// <response code="401">用户未认证</response>
        /// <remarks>
        /// 返回当前登录用户的完整权限信息，包括：
        /// - 用户所属的所有角色
        /// - 通过角色继承的权限
        /// - 直接分配给用户的权限
        /// - 最终生效的权限列表（合并去重后）
        /// </remarks>
        [HttpGet("me/permissions")]
        [ProducesResponseType(typeof(ApiResponse<UserPermissionInfoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
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
        /// <response code="200">成功返回权限检查结果</response>
        /// <response code="401">用户未认证</response>
        /// <remarks>
        /// 用于前端在执行敏感操作前检查用户是否具有相应权限。
        /// 检查逻辑会综合考虑角色权限和直接权限。
        /// </remarks>
        [HttpGet("me/permissions/check")]
        [ProducesResponseType(typeof(ApiResponse<UserPermissionCheckDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
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
        /// <response code="200">成功返回当前用户信息</response>
        /// <response code="401">用户未认证</response>
        /// <remarks>
        /// 返回当前登录用户的基本信息和权限摘要，包括：
        /// - 用户ID和用户名
        /// - 所属角色列表
        /// - 拥有的权限列表
        ///
        /// 常用于前端初始化用户状态和权限控制。
        /// </remarks>
        [HttpGet("me")]
        [ProducesResponseType(typeof(ApiResponse<CurrentUserInfoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
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

        /// <summary>
        /// 获取当前用户的菜单树
        /// </summary>
        /// <returns>菜单树</returns>
        [HttpGet("me/menus")]
        [ProducesResponseType(typeof(ApiResponse<List<MenuDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyMenus()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Error("用户未认证", StatusCodes.Status401Unauthorized);
            }

            var menus = await _menuService.GetMenusByUserAsync(userId);
            return Success(menus);
        }
    }
}
