import apiClient from '@/utils/request'
import type { User, UserLoginDto, UserRegisterDto, UserUpdateDto, ApiResponse } from '@/types/api'

/**
 * 用户API服务
 */
export class UserApi {
  /**
   * 用户登录
   */
  static async login(loginData: UserLoginDto): Promise<{ accessToken: string; refreshToken: string }> {
    const response = await apiClient.post<{ accessToken: string; refreshToken: string }>('/users/login', loginData)
    return response.data || { accessToken: '', refreshToken: '' }
  }

  /**
   * 用户注册
   */
  static async register(registerData: UserRegisterDto): Promise<User> {
    const response = await apiClient.post<User>('/users/register', registerData)
    return response.data || {} as User
  }

  /**
   * 获取当前用户信息
   */
  static async getProfile(): Promise<User> {
    const response = await apiClient.get<User>('/users/profile')
    return response.data || {} as User
  }

  /**
   * 获取所有用户列表
   */
  static async getAllUsers(): Promise<{ users: User[]; total: number }> {
    const response = await apiClient.get<{ users: User[]; total: number }>('/users')
    return response.data || { users: [], total: 0 }
  }

  /**
   * 根据ID获取用户
   */
  static async getUserById(id: number): Promise<User> {
    const response = await apiClient.get<User>(`/users/${id}`)
    return response.data || {} as User
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