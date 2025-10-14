using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApiWeb.Models.DTOs;
using MyApiWeb.Models.Entities;
using MyApiWeb.Services.Interfaces;

namespace MyApiWeb.Api.Controllers
{
    /// <summary>
    /// 权限管理控制器
    /// </summary>
    public class PermissionsController : ApiControllerBase<IPermissionService, Permission, string>
    {
        public PermissionsController(ILogger<PermissionsController> logger, IPermissionService permissionService)
            : base(logger, permissionService)
        {
        }

        /// <summary>
        /// 获取所有权限
        /// </summary>
        /// <returns>权限列表</returns>
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
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<PermissionDto>), StatusCodes.Status200OK)]
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
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PermissionDto>), StatusCodes.Status200OK)]
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
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<PermissionDto>), StatusCodes.Status200OK)]
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
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
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
        /// <param name="excludeId">排除的权限ID</param>
        /// <returns>检查结果</returns>
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
        /// <param name="checkPermissionDto">权限检查请求</param>
        /// <returns>权限检查结果列表</returns>
        [HttpPost("users/{userId}/check-batch")]
        [ProducesResponseType(typeof(ApiResponse<List<UserPermissionCheckDto>>), StatusCodes.Status200OK)]
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
        /// <param name="assignPermissionsDto">权限分配请求</param>
        /// <returns>分配结果</returns>
        [HttpPut("users/{userId}/permissions")]
        [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
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
        [HttpDelete("users/{userId}/permissions/{permissionId}")]
        [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
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
        [HttpGet("users/{userId}/direct")]
        [ProducesResponseType(typeof(ApiResponse<List<PermissionDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserDirectPermissions(string userId)
        {
            var permissions = await _service.GetUserDirectPermissionsAsync(userId);
            return Success(permissions);
        }
    }
}
