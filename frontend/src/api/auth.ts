import apiClient from '@/utils/request'
import type {
  CurrentUserInfoDto,
  UserPermissionInfoDto,
  UserPermissionCheckDto,
  MenuDto,
} from '@/types/api'

/**
 * 认证相关 API 服务
 */
export class AuthApi {
  /**
   * 获取当前用户信息
   */
  static async getCurrentUser(): Promise<CurrentUserInfoDto> {
    return apiClient.get('/auth/me')
  }

  /**
   * 获取当前用户权限信息
   */
  static async getCurrentUserPermissions(): Promise<UserPermissionInfoDto> {
    return apiClient.get('/auth/me/permissions')
  }

  /**
   * 检查当前用户是否有指定权限
   */
  static async checkCurrentUserPermission(permission: string): Promise<UserPermissionCheckDto> {
    return apiClient.get('/auth/me/permissions/check', {
      params: { permission },
    })
  }

  /**
   * 获取当前用户的菜单树
   */
  static async getCurrentUserMenus(): Promise<MenuDto[]> {
    return apiClient.get('/auth/me/menus')
  }
}

export default AuthApi
