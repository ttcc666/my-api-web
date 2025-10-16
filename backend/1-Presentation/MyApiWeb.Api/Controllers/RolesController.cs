using Microsoft.AspNetCore.Mvc;
using MyApiWeb.Models.DTOs;
using MyApiWeb.Models.Entities.System;
using MyApiWeb.Services.Interfaces.System;

namespace MyApiWeb.Api.Controllers
{
    /// <summary>
    /// 角色管理控制器
    /// </summary>
    /// <remarks>
    /// 提供角色的完整CRUD操作，以及角色权限管理和用户角色分配功能。
    /// 角色是权限管理的核心，用户通过角色继承权限。
    /// </remarks>
    public class RolesController : ApiControllerBase<IRoleService, Role, string>
    {
        /// <summary>
        /// 初始化角色管理控制器
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <param name="roleService">角色服务</param>
        public RolesController(ILogger<RolesController> logger, IRoleService roleService)
            : base(logger, roleService)
        {
        }

        /// <summary>
        /// 获取所有角色
        /// </summary>
        /// <returns>角色列表</returns>
        /// <response code="200">成功返回所有角色列表</response>
        /// <remarks>
        /// 返回系统中定义的所有角色，包括角色的名称、描述等信息。
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<RoleDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _service.GetAllRolesAsync();
            return Success(roles);
        }

        /// <summary>
        /// 根据ID获取角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>角色信息</returns>
        /// <response code="200">成功返回角色信息</response>
        /// <response code="404">角色不存在</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRoleById(string id)
        {
            var role = await _service.GetRoleByIdAsync(id);
            if (role == null)
            {
                return Error("角色不存在", StatusCodes.Status404NotFound);
            }

            return Success(role);
        }

        /// <summary>
        /// 创建角色
        /// </summary>
        /// <param name="createRoleDto">创建角色请求</param>
        /// <returns>创建的角色信息</returns>
        /// <response code="201">角色创建成功</response>
        /// <response code="400">请求参数无效</response>
        /// <remarks>
        /// 创建新的角色定义。角色名称必须唯一。
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto createRoleDto)
        {
            if (!ModelState.IsValid)
            {
                return Error("请求参数无效", StatusCodes.Status400BadRequest);
            }

            var role = await _service.CreateRoleAsync(createRoleDto);
            return Success(role, "角色创建成功", StatusCodes.Status201Created);
        }

        /// <summary>
        /// 更新角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <param name="updateRoleDto">更新角色请求</param>
        /// <returns>更新的角色信息</returns>
        /// <response code="200">角色更新成功</response>
        /// <response code="400">请求参数无效</response>
        /// <response code="404">角色不存在</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateRole(string id, [FromBody] UpdateRoleDto updateRoleDto)
        {
            if (!ModelState.IsValid)
            {
                return Error("请求参数无效", StatusCodes.Status400BadRequest);
            }

            var role = await _service.UpdateRoleAsync(id, updateRoleDto);
            return Success(role);
        }

        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>删除结果</returns>
        /// <response code="200">角色删除成功</response>
        /// <response code="400">角色删除失败</response>
        /// <response code="404">角色不存在</response>
        /// <remarks>
        /// 删除角色会同时移除所有相关的用户角色关联和角色权限关联。
        /// </remarks>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var result = await _service.DeleteRoleAsync(id);
            if (result)
            {
                return SuccessMessage("角色删除成功");
            }

            return Error("角色删除失败", StatusCodes.Status400BadRequest);
        }

        /// <summary>
        /// 为角色分配权限
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <param name="assignPermissionsDto">权限分配请求（包含权限ID列表）</param>
        /// <returns>分配结果</returns>
        /// <response code="200">权限分配成功</response>
        /// <response code="400">请求参数无效或分配失败</response>
        /// <remarks>
        /// 为角色分配权限，会替换角色现有的所有权限。
        /// 拥有该角色的所有用户将自动继承这些权限。
        /// </remarks>
        [HttpPut("{id}/permissions")]
        [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AssignPermissionsToRole(string id, [FromBody] AssignRolePermissionsDto assignPermissionsDto)
        {
            if (!ModelState.IsValid)
            {
                return Error("请求参数无效", StatusCodes.Status400BadRequest);
            }

            var result = await _service.AssignPermissionsToRoleAsync(id, assignPermissionsDto);
            if (result)
            {
                return SuccessMessage("权限分配成功");
            }

            return Error("权限分配失败", StatusCodes.Status400BadRequest);
        }

        /// <summary>
        /// 获取角色的权限列表
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>权限列表</returns>
        /// <response code="200">成功返回角色的权限列表</response>
        /// <remarks>
        /// 返回指定角色拥有的所有权限。
        /// </remarks>
        [HttpGet("{id}/permissions")]
        [ProducesResponseType(typeof(ApiResponse<List<PermissionDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRolePermissions(string id)
        {
            var permissions = await _service.GetRolePermissionsAsync(id);
            return Success(permissions);
        }

        /// <summary>
        /// 检查角色名称是否存在
        /// </summary>
        /// <param name="name">角色名称</param>
        /// <param name="excludeId">排除的角色ID（用于更新时排除自身）</param>
        /// <returns>检查结果</returns>
        /// <response code="200">成功返回检查结果</response>
        /// <remarks>
        /// 用于创建或更新角色时验证名称唯一性。
        /// 更新时可通过 excludeId 参数排除当前角色自身。
        /// </remarks>
        [HttpGet("check-name")]
        [ProducesResponseType(typeof(ApiResponse<ExistsResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckRoleName([FromQuery] string name, [FromQuery] string? excludeId = null)
        {
            var exists = await _service.RoleNameExistsAsync(name, excludeId);
            return Success(new ExistsResponseDto { Exists = exists });
        }
    }
}
