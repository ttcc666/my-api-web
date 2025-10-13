import apiClient from '@/utils/request'
import type { ApiResponse, Role, Permission } from '@/types/api'

/**
 * 角色创建DTO
 */
export interface RoleCreateDto {
  name: string
  description?: string
  permissionIds: string[]
}

/**
 * 角色更新DTO
 */
export interface RoleUpdateDto {
  name: string
  description?: string
  isEnabled: boolean
  permissionIds: string[]
}

/**
 * 角色API服务
 */
export class RoleApi {
  /**
   * 获取所有角色列表
   */
  static async getAllRoles(): Promise<Role[]> {
    const response = await apiClient.get<Role[]>('/roles')
    return response.data || []
  }

  /**
   * 根据ID获取角色
   */
  static async getRoleById(id: string): Promise<Role> {
    const response = await apiClient.get<Role>(`/roles/${id}`)
    return response.data
  }

  /**
   * 创建新角色
   */
  static async createRole(roleData: RoleCreateDto): Promise<Role> {
    const response = await apiClient.post<Role>('/roles', roleData)
    return response.data
  }

  /**
   * 更新角色信息
   */
  static async updateRole(id: string, roleData: RoleUpdateDto): Promise<void> {
    await apiClient.put(`/roles/${id}`, roleData)
  }

  /**
   * 删除角色
   */
  static async deleteRole(id: string): Promise<void> {
    await apiClient.delete(`/roles/${id}`)
  }

  /**
   * 获取所有权限列表
   */
  static async getAllPermissions(): Promise<Permission[]> {
    const response = await apiClient.get<Permission[]>('/permissions')
    return response.data || []
  }
}

export default RoleApi