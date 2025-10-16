/**
 * 认证相关类型定义
 * Authentication Related Type Definitions
 */

/**
 * 用户登录DTO
 */
export interface UserLoginDto {
  username: string
  password: string
}

/**
 * 当前用户信息DTO
 */
export interface CurrentUserInfoDto {
  id: string
  username: string
  permissions: string[]
  roles: string[]
}
