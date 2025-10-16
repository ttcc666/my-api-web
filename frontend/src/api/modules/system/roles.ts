import apiClient from '@/utils/request'
import type {
  RoleDto,
  CreateRoleDto,
  UpdateRoleDto,
  PermissionDto,
  AssignRolePermissionsDto,
  ExistsResponseDto,
} from '@/types/api'

/**
 * 角色相关 API 服务
 */
export class RolesApi {
  /**
   * 获取所有角色列表
   */
  static async getAllRoles(): Promise<RoleDto[]> {
    return apiClient.get('/roles')
  }

  /**
   * 根据ID获取角色
   */
  static async getRoleById(id: string): Promise<RoleDto> {
    return apiClient.get(`/roles/${id}`)
  }

  /**
   * 创建新角色
   */
  static async createRole(roleData: CreateRoleDto): Promise<RoleDto> {
    return apiClient.post('/roles', roleData)
  }

  /**
   * 更新角色信息
   */
  static async updateRole(id: string, roleData: UpdateRoleDto): Promise<RoleDto> {
    return apiClient.put(`/roles/${id}`, roleData)
  }

  /**
   * 删除角色
   */
  static async deleteRole(id: string): Promise<void> {
    return apiClient.delete(`/roles/${id}`)
  }

  /**
   * 分配角色权限
   */
  static async assignRolePermissions(
    id: string,
    permissionsData: AssignRolePermissionsDto,
  ): Promise<void> {
    return apiClient.put(`/roles/${id}/permissions`, permissionsData)
  }

  /**
   * 获取角色的权限列表
   */
  static async getRolePermissions(id: string): Promise<PermissionDto[]> {
    return apiClient.get(`/roles/${id}/permissions`)
  }

  /**
   * 检查角色名称是否已存在
   */
  static async checkRoleName(name: string, excludeId?: string): Promise<ExistsResponseDto> {
    const params: Record<string, string> = { name }
    if (excludeId) {
      params.excludeId = excludeId
    }
    return apiClient.get('/roles/check-name', { params })
  }
}

export default RolesApi
