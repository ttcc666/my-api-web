import { UsersApi } from './users'
import { TokenApi } from './token'
import { AuthApi } from './auth'
import type {
  UserDto,
  UserLoginDto,
  UserRegisterDto,
  UserUpdateDto,
  UserPermissionInfoDto,
  TokenDto,
} from '@/types/api'

/**
 * 用户API服务 (向后兼容性包装器)
 * @deprecated 请使用新的模块化 API: UsersApi, AuthApi, TokenApi
 */
export class UserApi {
  /**
   * 用户登录
   */
  static async login(loginData: UserLoginDto): Promise<TokenDto> {
    return UsersApi.login(loginData)
  }

  /**
   * 用户注册
   */
  static async register(registerData: UserRegisterDto): Promise<UserDto> {
    return UsersApi.register(registerData)
  }

  /**
   * 获取当前用户信息
   */
  static async getProfile(): Promise<UserDto> {
    return UsersApi.getProfile()
  }

  /**
   * 获取当前用户的权限信息
   */
  static async getUserPermissions(): Promise<UserPermissionInfoDto> {
    return AuthApi.getCurrentUserPermissions()
  }

  /**
   * 获取所有用户列表
   */
  static async getAllUsers(): Promise<UserDto[]> {
    return UsersApi.getAllUsers()
  }

  /**
   * 根据ID获取用户
   */
  static async getUserById(id: string): Promise<UserDto> {
    return UsersApi.getUserById(id)
  }

  /**
   * 更新用户信息
   */
  static async updateUser(id: string, updateData: UserUpdateDto): Promise<void> {
    return UsersApi.updateUser(id, updateData)
  }

  /**
   * 删除用户
   */
  static async deleteUser(id: string): Promise<void> {
    return UsersApi.deleteUser(id)
  }

  /**
   * 用户登出
   */
  static async logout(refreshToken: string | null): Promise<void> {
    if (refreshToken) {
      try {
        await TokenApi.logout({ refreshToken })
      } catch (error) {
        console.error('Failed to revoke token on server:', error)
      }
    }
    // 清理本地存储的逻辑由 authStore 处理
  }

  /**
   * 检查是否已登录
   */
  static isLoggedIn(): boolean {
    return !!localStorage.getItem('token')
  }

  /**
   * 获取存储的token
   */
  static getToken(): string | null {
    return localStorage.getItem('token')
  }
}

// 默认导出
export default UserApi
