import apiClient from '@/utils/request'
import type { User, UserLoginDto, UserRegisterDto, UserUpdateDto, ApiResponse, UserPermissionInfo } from '@/types/api'

/**
 * 用户API服务
 */
export class UserApi {
  /**
   * 用户登录
   */
  static async login(loginData: UserLoginDto): Promise<{ accessToken: string; refreshToken: string }> {
    // 拦截器已经处理了数据解包，所以这里直接返回 Promise 的结果
    return apiClient.post('/users/login', loginData);
  }

  /**
   * 用户注册
   */
  static async register(registerData: UserRegisterDto): Promise<User> {
    // 对于 register，同样直接返回，让拦截器处理
    return apiClient.post('/users/register', registerData);
  }

  /**
   * 获取当前用户信息
   */
  static async getProfile(): Promise<User> {
    // 对于 getProfile，同样直接返回
    return apiClient.get('/users/profile');
  }

  /**
   * 获取当前用户的权限信息
   */
  static async getUserPermissions(): Promise<UserPermissionInfo> {
    // 对于 getUserPermissions，同样直接返回
    return apiClient.get('/auth/me/permissions');
  }

  /**
   * 获取所有用户列表
   */
  static async getAllUsers(): Promise<{ users: User[]; total: number }> {
    // 对于 getAllUsers，同样直接返回
    return apiClient.get('/users');
  }

  /**
   * 根据ID获取用户
   */
  static async getUserById(id: number): Promise<User> {
    // 对于 getUserById，同样直接返回
    return apiClient.get(`/users/${id}`);
  }

  /**
   * 更新用户信息
   */
  static async updateUser(id: number, updateData: UserUpdateDto): Promise<void> {
    await apiClient.put(`/users/${id}`, updateData)
  }

  /**
   * 删除用户
   */
  static async deleteUser(id: number): Promise<void> {
    await apiClient.delete(`/users/${id}`)
  }

  /**
   * 用户登出
   */
  static async logout(refreshToken: string | null): Promise<void> {
    if (refreshToken) {
      try {
        await apiClient.post('/token/logout', { refreshToken })
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