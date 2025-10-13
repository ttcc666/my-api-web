using MyApiWeb.Models.DTOs;
using MyApiWeb.Models.Entities;

namespace MyApiWeb.Services.Interfaces
{
    /// <summary>
    /// 角色服务接口
    /// </summary>
    public interface IRoleService
    {
        /// <summary>
        /// 获取所有角色
        /// </summary>
        /// <returns>角色列表</returns>
        Task<List<RoleDto>> GetAllRolesAsync();

        /// <summary>
        /// 根据ID获取角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>角色信息</returns>
        Task<RoleDto?> GetRoleByIdAsync(string id);

        /// <summary>
        /// 创建角色
        /// </summary>
        /// <param name="createRoleDto">创建角色请求</param>
        /// <returns>创建的角色信息</returns>
        Task<RoleDto> CreateRoleAsync(CreateRoleDto createRoleDto);

        /// <summary>
        /// 更新角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <param name="updateRoleDto">更新角色请求</param>
        /// <returns>更新的角色信息</returns>
        Task<RoleDto> UpdateRoleAsync(string id, UpdateRoleDto updateRoleDto);

        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>是否删除成功</returns>
        Task<bool> DeleteRoleAsync(string id);

        /// <summary>
        /// 为角色分配权限
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <param name="assignPermissionsDto">权限分配请求</param>
        /// <returns>是否分配成功</returns>
        Task<bool> AssignPermissionsToRoleAsync(string roleId, AssignRolePermissionsDto assignPermissionsDto);

        /// <summary>
        /// 获取角色的权限列表
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <returns>权限列表</returns>
        Task<List<PermissionDto>> GetRolePermissionsAsync(string roleId);

        /// <summary>
        /// 检查角色名称是否存在
        /// </summary>
        /// <param name="name">角色名称</param>
        /// <param name="excludeId">排除的角色ID（用于更新时检查）</param>
        /// <returns>是否存在</returns>
        Task<bool> RoleNameExistsAsync(string name, string? excludeId = null);

        /// <summary>
        /// 获取用户的角色列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>角色列表</returns>
        Task<List<RoleDto>> GetUserRolesAsync(string userId);

        /// <summary>
        /// 为用户分配角色
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="assignRolesDto">角色分配请求</param>
        /// <returns>是否分配成功</returns>
        Task<bool> AssignRolesToUserAsync(string userId, AssignUserRolesDto assignRolesDto);

        /// <summary>
        /// 移除用户的角色
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="roleId">角色ID</param>
        /// <returns>是否移除成功</returns>
        Task<bool> RemoveRoleFromUserAsync(string userId, string roleId);
    }
}