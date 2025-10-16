using Microsoft.AspNetCore.Mvc;
using MyApiWeb.Models.DTOs;
using MyApiWeb.Models.Entities.System;
using MyApiWeb.Services.Interfaces.System;

namespace MyApiWeb.Api.Controllers
{
    /// <summary>
    /// 权限管理控制器
    /// </summary>
    /// <remarks>
    /// 提供权限的完整CRUD操作，以及用户权限的查询、检查和分配功能。
    /// 支持权限分组管理和批量权限检查。
    /// </remarks>
    public class PermissionsController : ApiControllerBase<IPermissionService, Permission, string>
    {
        /// <summary>
        /// 初始化权限管理控制器
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <param name="permissionService">权限服务</param>
        public PermissionsController(ILogger<PermissionsController> logger, IPermissionService permissionService)
            : base(logger, permissionService)
        {
        }

        /// <summary>
        /// 获取所有权限
        /// </summary>
        /// <returns>权限列表</returns>
        /// <response code="200">成功返回所有权限列表</response>
        /// <remarks>
        /// 返回系统中定义的所有权限，包括权限的名称、描述、分组等信息。
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<PermissionDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllPermissions()
        {
            var permissions = await _service.GetAllPermissionsAsync();
            return Success(permissions);
        }

        /// <summary>
        /// 按分组获取权限
        /// </summary>
        /// <returns>按分组组织的权限列表</returns>
        /// <response code="200">成功返回按分组组织的权限列表</response>
        /// <remarks>
        /// 将权限按照分组（Group）字段进行归类，便于前端展示和管理。
        /// 适用于权限选择器等场景。
        /// </remarks>
        [HttpGet("groups")]
        [ProducesResponseType(typeof(ApiResponse<List<PermissionGroupDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPermissionsByGroup()
        {
            var groups = await _service.GetPermissionsByGroupAsync();
            return Success(groups);
        }

        /// <summary>
        /// 根据ID获取权限
        /// </summary>
        /// <param name="id">权限ID</param>
        /// <returns>权限信息</returns>
        /// <response code="200">成功返回权限信息</response>
        /// <response code="404">权限不存在</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<PermissionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPermissionById(string id)
        {
            var permission = await _service.GetPermissionByIdAsync(id);
            if (permission == null)
            {
                return Error("权限不存在", StatusCodes.Status404NotFound);
            }

            return Success(permission);
        }

        /// <summary>
        /// 创建权限
        /// </summary>
        /// <param name="createPermissionDto">创建权限请求</param>
        /// <returns>创建的权限信息</returns>
        /// <response code="201">权限创建成功</response>
        /// <response code="400">请求参数无效</response>
        /// <remarks>
        /// 创建新的权限定义。权限名称必须唯一。
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PermissionDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionDto createPermissionDto)
        {
            if (!ModelState.IsValid)
            {
                return Error("请求参数无效", StatusCodes.Status400BadRequest);
            }

            var permission = await _service.CreatePermissionAsync(createPermissionDto);
            return Success(permission, "权限创建成功", StatusCodes.Status201Created);
        }

        /// <summary>
        /// 更新权限
        /// </summary>
        /// <param name="id">权限ID</param>
        /// <param name="updatePermissionDto">更新权限请求</param>
        /// <returns>更新的权限信息</returns>
        /// <response code="200">权限更新成功</response>
        /// <response code="400">请求参数无效</response>
        /// <response code="404">权限不存在</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<PermissionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePermission(string id, [FromBody] UpdatePermissionDto updatePermissionDto)
        {
            if (!ModelState.IsValid)
            {
                return Error("请求参数无效", StatusCodes.Status400BadRequest);
            }

            var permission = await _service.UpdatePermissionAsync(id, updatePermissionDto);
            return Success(permission);
        }

        /// <summary>
        /// 删除权限
        /// </summary>
        /// <param name="id">权限ID</param>
        /// <returns>删除结果</returns>
        /// <response code="200">权限删除成功</response>
        /// <response code="400">权限删除失败</response>
        /// <response code="404">权限不存在</response>
        /// <remarks>
        /// 删除权限会同时移除所有相关的角色权限和用户权限关联。
        /// </remarks>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeletePermission(string id)
        {
            var result = await _service.DeletePermissionAsync(id);
            if (result)
            {
                return SuccessMessage("权限删除成功");
            }

            return Error("权限删除失败", StatusCodes.Status400BadRequest);
        }

        /// <summary>
        /// 检查权限名称是否存在
        /// </summary>
        /// <param name="name">权限名称</param>
        /// <param name="excludeId">排除的权限ID（用于更新时排除自身）</param>
        /// <returns>检查结果</returns>
        /// <response code="200">成功返回检查结果</response>
        /// <remarks>
        /// 用于创建或更新权限时验证名称唯一性。
        /// 更新时可通过 excludeId 参数排除当前权限自身。
        /// </remarks>
        [HttpGet("check-name")]
        [ProducesResponseType(typeof(ApiResponse<ExistsResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckPermissionName([FromQuery] string name, [FromQuery] string? excludeId = null)
        {
            var exists = await _service.PermissionNameExistsAsync(name, excludeId);
            return Success(new ExistsResponseDto { Exists = exists });
        }

        /// <summary>
        /// 获取用户权限信息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>用户权限信息</returns>
        /// <response code="200">成功返回用户权限信息</response>
        /// <remarks>
        /// 返回指定用户的完整权限信息，包括角色权限、直接权限和最终生效权限。
        /// </remarks>
        [HttpGet("users/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<UserPermissionInfoDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserPermissions(string userId)
        {
            var userPermissions = await _service.GetUserPermissionsAsync(userId);
            return Success(userPermissions);
        }

        /// <summary>
        /// 检查用户权限
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="permission">权限名称</param>
        /// <returns>权限检查结果</returns>
        /// <response code="200">成功返回权限检查结果</response>
        /// <remarks>
        /// 检查指定用户是否拥有某个权限，综合考虑角色权限和直接权限。
        /// </remarks>
        [HttpGet("users/{userId}/check")]
        [ProducesResponseType(typeof(ApiResponse<UserPermissionCheckDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckUserPermission(string userId, [FromQuery] string permission)
        {
            var result = await _service.CheckUserPermissionAsync(userId, permission);
            return Success(result);
        }

        /// <summary>
        /// 批量检查用户权限
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="checkPermissionDto">权限检查请求（包含多个权限名称）</param>
        /// <returns>权限检查结果列表</returns>
        /// <response code="200">成功返回批量检查结果</response>
        /// <response code="400">请求参数无效</response>
        /// <remarks>
        /// 一次性检查用户是否拥有多个权限，提高检查效率。
        /// </remarks>
        [HttpPost("users/{userId}/check-batch")]
        [ProducesResponseType(typeof(ApiResponse<List<UserPermissionCheckDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CheckUserPermissions(string userId, [FromBody] CheckPermissionDto checkPermissionDto)
        {
            if (!ModelState.IsValid)
            {
                return Error("请求参数无效", StatusCodes.Status400BadRequest);
            }

            var results = await _service.CheckUserPermissionsAsync(userId, checkPermissionDto);
            return Success(results);
        }

        /// <summary>
        /// 为用户分配直接权限
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="assignPermissionsDto">权限分配请求（包含权限ID列表）</param>
        /// <returns>分配结果</returns>
        /// <response code="200">权限分配成功</response>
        /// <response code="400">请求参数无效或分配失败</response>
        /// <remarks>
        /// 为用户直接分配权限，这些权限独立于角色权限。
        /// 会替换用户现有的所有直接权限。
        /// </remarks>
        [HttpPut("users/{userId}/permissions")]
        [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AssignPermissionsToUser(string userId, [FromBody] AssignUserPermissionsDto assignPermissionsDto)
        {
            if (!ModelState.IsValid)
            {
                return Error("请求参数无效", StatusCodes.Status400BadRequest);
            }

            var result = await _service.AssignPermissionsToUserAsync(userId, assignPermissionsDto);
            if (result)
            {
                return SuccessMessage("权限分配成功");
            }

            return Error("权限分配失败", StatusCodes.Status400BadRequest);
        }

        /// <summary>
        /// 移除用户的直接权限
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="permissionId">权限ID</param>
        /// <returns>移除结果</returns>
        /// <response code="200">权限移除成功</response>
        /// <response code="400">权限移除失败</response>
        /// <remarks>
        /// 移除用户的单个直接权限。不影响通过角色继承的权限。
        /// </remarks>
        [HttpDelete("users/{userId}/permissions/{permissionId}")]
        [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemovePermissionFromUser(string userId, string permissionId)
        {
            var result = await _service.RemovePermissionFromUserAsync(userId, permissionId);
            if (result)
            {
                return SuccessMessage("权限移除成功");
            }

            return Error("权限移除失败", StatusCodes.Status400BadRequest);
        }

        /// <summary>
        /// 获取用户的直接权限列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>直接权限列表</returns>
        /// <response code="200">成功返回用户的直接权限列表</response>
        /// <remarks>
        /// 仅返回直接分配给用户的权限，不包括通过角色继承的权限。
        /// </remarks>
        [HttpGet("users/{userId}/direct")]
        [ProducesResponseType(typeof(ApiResponse<List<PermissionDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserDirectPermissions(string userId)
        {
            var permissions = await _service.GetUserDirectPermissionsAsync(userId);
            return Success(permissions);
        }
    }
}
