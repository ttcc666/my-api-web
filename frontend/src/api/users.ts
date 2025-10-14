import apiClient from '@/utils/request'
import type {
  UserDto,
  UserRegisterDto,
  UserLoginDto,
  UserUpdateDto,
  TokenDto,
  RoleDto,
  AssignUserRolesDto,
} from '@/types/api'

/**
 * 用户相关 API 服务
 */
export class UsersApi {
  /**
   * 用户注册
   */
  static async register(registerData: UserRegisterDto): Promise<UserDto> {
    return apiClient.post('/users/register', registerData)
  }

  /**
   * 用户登录
   */
  static async login(loginData: UserLoginDto): Promise<TokenDto> {
    return apiClient.post('/users/login', loginData)
  }

  /**
   * 获取当前用户资料
   */
  static async getProfile(): Promise<UserDto> {
    return apiClient.get('/users/profile')
  }

  /**
   * 获取所有用户列表
   */
  static async getAllUsers(): Promise<UserDto[]> {
    return apiClient.get('/users')
  }

  /**
   * 根据ID获取用户
   */
  static async getUserById(id: string): Promise<UserDto> {
    return apiClient.get(`/users/${id}`)
  }

  /**
   * 更新用户信息
   */
  static async updateUser(id: string, updateData: UserUpdateDto): Promise<void> {
    return apiClient.put(`/users/${id}`, updateData)
  }

  /**
   * 删除用户
   */
  static async deleteUser(id: string): Promise<void> {
    return apiClient.delete(`/users/${id}`)
  }

  /**
   * 获取用户的角色列表
   */
  static async getUserRoles(id: string): Promise<RoleDto[]> {
    return apiClient.get(`/users/${id}/roles`)
  }

  /**
   * 分配用户角色
   */
  static async assignUserRoles(id: string, rolesData: AssignUserRolesDto): Promise<void> {
    return apiClient.put(`/users/${id}/roles`, rolesData)
  }

  /**
   * 移除用户的指定角色
   */
  static async removeUserRole(userId: string, roleId: string): Promise<void> {
    return apiClient.delete(`/users/${userId}/roles/${roleId}`)
  }
}

export default UsersApi
