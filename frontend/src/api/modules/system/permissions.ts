import apiClient from '@/utils/request'
import type {
  PermissionDto,
  CreatePermissionDto,
  UpdatePermissionDto,
  PermissionGroupDto,
  UserPermissionInfoDto,
  UserPermissionCheckDto,
  CheckPermissionDto,
  AssignUserPermissionsDto,
  ExistsResponseDto,
} from '@/types/api'

/**
 * 权限相关 API 服务
 */
export class PermissionsApi {
  /**
   * 获取所有权限列表
   */
  static async getAllPermissions(): Promise<PermissionDto[]> {
    return apiClient.get('/permissions')
  }

  /**
   * 根据ID获取权限
   */
  static async getPermissionById(id: string): Promise<PermissionDto> {
    return apiClient.get(`/permissions/${id}`)
  }

  /**
   * 创建新权限
   */
  static async createPermission(permissionData: CreatePermissionDto): Promise<PermissionDto> {
    return apiClient.post('/permissions', permissionData)
  }

  /**
   * 更新权限信息
   */
  static async updatePermission(
    id: string,
    permissionData: UpdatePermissionDto,
  ): Promise<PermissionDto> {
    return apiClient.put(`/permissions/${id}`, permissionData)
  }

  /**
   * 删除权限
   */
  static async deletePermission(id: string): Promise<void> {
    return apiClient.delete(`/permissions/${id}`)
  }

  /**
   * 获取权限分组列表
   */
  static async getPermissionGroups(): Promise<PermissionGroupDto[]> {
    return apiClient.get('/permissions/groups')
  }

  /**
   * 检查权限名称是否已存在
   */
  static async checkPermissionName(name: string, excludeId?: string): Promise<ExistsResponseDto> {
    const params: Record<string, string> = { name }
    if (excludeId) {
      params.excludeId = excludeId
    }
    return apiClient.get('/permissions/check-name', { params })
  }

  /**
   * 获取用户权限信息
   */
  static async getUserPermissions(userId: string): Promise<UserPermissionInfoDto> {
    return apiClient.get(`/permissions/users/${userId}`)
  }

  /**
   * 检查用户是否有指定权限
   */
  static async checkUserPermission(
    userId: string,
    permission: string,
  ): Promise<UserPermissionCheckDto> {
    return apiClient.get(`/permissions/users/${userId}/check`, {
      params: { permission },
    })
  }

  /**
   * 批量检查用户权限
   */
  static async checkUserPermissionsBatch(
    userId: string,
    permissions: CheckPermissionDto,
  ): Promise<UserPermissionCheckDto[]> {
    return apiClient.post(`/permissions/users/${userId}/check-batch`, permissions)
  }

  /**
   * 分配用户权限
   */
  static async assignUserPermissions(
    userId: string,
    permissionsData: AssignUserPermissionsDto,
  ): Promise<void> {
    return apiClient.put(`/permissions/users/${userId}/permissions`, permissionsData)
  }

  /**
   * 移除用户的指定权限
   */
  static async removeUserPermission(userId: string, permissionId: string): Promise<void> {
    return apiClient.delete(`/permissions/users/${userId}/permissions/${permissionId}`)
  }

  /**
   * 获取用户的直接权限列表
   */
  static async getUserDirectPermissions(userId: string): Promise<PermissionDto[]> {
    return apiClient.get(`/permissions/users/${userId}/direct`)
  }
}

export default PermissionsApi
