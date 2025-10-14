using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApiWeb.Models.DTOs;
using MyApiWeb.Models.Entities;
using MyApiWeb.Services.Interfaces;

namespace MyApiWeb.Api.Controllers
{
    /// <summary>
    /// 角色管理控制器
    /// </summary>
    public class RolesController : ApiControllerBase<IRoleService, Role, string>
    {
        public RolesController(ILogger<RolesController> logger, IRoleService roleService)
            : base(logger, roleService)
        {
        }

        /// <summary>
        /// 获取所有角色
        /// </summary>
        /// <returns>角色列表</returns>
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
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status200OK)]
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
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status200OK)]
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
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status200OK)]
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
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
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
        /// <param name="assignPermissionsDto">权限分配请求</param>
        /// <returns>分配结果</returns>
        [HttpPut("{id}/permissions")]
        [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
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
        /// <param name="excludeId">排除的角色ID</param>
        /// <returns>检查结果</returns>
        [HttpGet("check-name")]
        [ProducesResponseType(typeof(ApiResponse<ExistsResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckRoleName([FromQuery] string name, [FromQuery] string? excludeId = null)
        {
            var exists = await _service.RoleNameExistsAsync(name, excludeId);
            return Success(new ExistsResponseDto { Exists = exists });
        }
    }
}
