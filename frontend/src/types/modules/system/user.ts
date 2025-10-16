/**
 * 用户相关类型定义
 * User Related Type Definitions
 */

/**
 * 用户信息
 */
export interface UserDto {
  id: string
  username: string
  email: string
  realName?: string
  phone?: string
  isActive: boolean
  createdTime: string
  lastLoginTime?: string
}

/**
 * 用户注册DTO
 */
export interface UserRegisterDto {
  username: string
  email: string
  password: string
  realName?: string
  phone?: string
}

/**
 * 用户更新DTO
 */
export interface UserUpdateDto {
  realName?: string
  phone?: string
}

/**
 * 修改密码DTO
 */
export interface ChangePasswordDto {
  currentPassword: string
  newPassword: string
}

/**
 * 分配用户角色DTO
 */
export interface AssignUserRolesDto {
  roleIds: string[]
}

/**
 * @deprecated 使用 UserDto 替代
 */
export type User = UserDto
