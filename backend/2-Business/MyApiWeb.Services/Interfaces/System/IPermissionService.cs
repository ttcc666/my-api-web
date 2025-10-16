using MyApiWeb.Models.DTOs;
using MyApiWeb.Models.Entities.Common;
using MyApiWeb.Models.Entities.System;
using MyApiWeb.Models.Entities.Auth;
using MyApiWeb.Models.Entities.Hub;

namespace MyApiWeb.Services.Interfaces.System
{
    /// <summary>
    /// 权限服务接口
    /// </summary>
    public interface IPermissionService
    {
        /// <summary>
        /// 获取所有权限
        /// </summary>
        /// <returns>权限列表</returns>
        Task<List<PermissionDto>> GetAllPermissionsAsync();

        /// <summary>
        /// 根据分组获取权限
        /// </summary>
        /// <returns>按分组组织的权限列表</returns>
        Task<List<PermissionGroupDto>> GetPermissionsByGroupAsync();

        /// <summary>
        /// 根据ID获取权限
        /// </summary>
        /// <param name="id">权限ID</param>
        /// <returns>权限信息</returns>
        Task<PermissionDto?> GetPermissionByIdAsync(string id);

        /// <summary>
        /// 创建权限
        /// </summary>
        /// <param name="createPermissionDto">创建权限请求</param>
        /// <returns>创建的权限信息</returns>
        Task<PermissionDto> CreatePermissionAsync(CreatePermissionDto createPermissionDto);

        /// <summary>
        /// 更新权限
        /// </summary>
        /// <param name="id">权限ID</param>
        /// <param name="updatePermissionDto">更新权限请求</param>
        /// <returns>更新的权限信息</returns>
        Task<PermissionDto> UpdatePermissionAsync(string id, UpdatePermissionDto updatePermissionDto);

        /// <summary>
        /// 删除权限
        /// </summary>
        /// <param name="id">权限ID</param>
        /// <returns>是否删除成功</returns>
        Task<bool> DeletePermissionAsync(string id);

        /// <summary>
        /// 检查权限名称是否存在
        /// </summary>
        /// <param name="name">权限名称</param>
        /// <param name="excludeId">排除的权限ID（用于更新时检查）</param>
        /// <returns>是否存在</returns>
        Task<bool> PermissionNameExistsAsync(string name, string? excludeId = null);

        /// <summary>
        /// 获取用户的最终权限列表（角色权限 + 直接权限的并集）
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>用户权限信息</returns>
        Task<UserPermissionInfoDto> GetUserPermissionsAsync(string userId);

        /// <summary>
        /// 检查用户是否拥有指定权限
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="permission">权限名称</param>
        /// <returns>权限检查结果</returns>
        Task<UserPermissionCheckDto> CheckUserPermissionAsync(string userId, string permission);

        /// <summary>
        /// 批量检查用户权限
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="checkPermissionDto">权限检查请求</param>
        /// <returns>权限检查结果列表</returns>
        Task<List<UserPermissionCheckDto>> CheckUserPermissionsAsync(string userId, CheckPermissionDto checkPermissionDto);

        /// <summary>
        /// 为用户直接分配权限
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="assignPermissionsDto">权限分配请求</param>
        /// <returns>是否分配成功</returns>
        Task<bool> AssignPermissionsToUserAsync(string userId, AssignUserPermissionsDto assignPermissionsDto);

        /// <summary>
        /// 移除用户的直接权限
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="permissionId">权限ID</param>
        /// <returns>是否移除成功</returns>
        Task<bool> RemovePermissionFromUserAsync(string userId, string permissionId);

        /// <summary>
        /// 获取用户的直接权限列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>直接权限列表</returns>
        Task<List<PermissionDto>> GetUserDirectPermissionsAsync(string userId);
    }
}
