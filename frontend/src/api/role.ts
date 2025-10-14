import { RolesApi } from './roles'
import { PermissionsApi } from './permissions'
import type { RoleDto, CreateRoleDto, UpdateRoleDto, PermissionDto } from '@/types/api'

/**
 * 角色创建DTO (向后兼容)
 * @deprecated 使用 CreateRoleDto 替代
 */
export interface RoleCreateDto extends CreateRoleDto {
  permissionIds: string[]
}

/**
 * 角色更新DTO (向后兼容)
 * @deprecated 使用 UpdateRoleDto 替代
 */
export interface RoleUpdateDto extends UpdateRoleDto {
  permissionIds: string[]
}

/**
 * 角色API服务 (向后兼容性包装器)
 * @deprecated 请使用新的模块化 API: RolesApi, PermissionsApi
 */
export class RoleApi {
  /**
   * 获取所有角色列表
   */
  static async getAllRoles(): Promise<RoleDto[]> {
    return RolesApi.getAllRoles()
  }

  /**
   * 根据ID获取角色
   */
  static async getRoleById(id: string): Promise<RoleDto> {
    return RolesApi.getRoleById(id)
  }

  /**
   * 创建新角色
   */
  static async createRole(roleData: RoleCreateDto): Promise<RoleDto> {
    const createData: CreateRoleDto = {
      name: roleData.name,
      description: roleData.description,
      isEnabled: roleData.isEnabled,
      permissionIds: roleData.permissionIds,
    }
    return RolesApi.createRole(createData)
  }

  /**
   * 更新角色信息
   */
  static async updateRole(id: string, roleData: RoleUpdateDto): Promise<void> {
    const updateData: UpdateRoleDto = {
      name: roleData.name,
      description: roleData.description,
      isEnabled: roleData.isEnabled,
    }
    await RolesApi.updateRole(id, updateData)

    // 如果提供了权限ID，则更新角色权限
    if (roleData.permissionIds) {
      await RolesApi.assignRolePermissions(id, { permissionIds: roleData.permissionIds })
    }
  }

  /**
   * 删除角色
   */
  static async deleteRole(id: string): Promise<void> {
    return RolesApi.deleteRole(id)
  }

  /**
   * 获取所有权限列表
   */
  static async getAllPermissions(): Promise<PermissionDto[]> {
    return PermissionsApi.getAllPermissions()
  }
}

export default RoleApi
