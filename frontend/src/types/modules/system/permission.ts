/**
 * 权限相关类型定义
 * Permission Related Type Definitions
 */

import type { RoleDto } from './role'

/**
 * 权限信息
 */
export interface PermissionDto {
  id: string
  name: string
  displayName: string
  description?: string
  group?: string
  isEnabled: boolean
  creationTime: string
}

/**
 * 创建权限DTO
 */
export interface CreatePermissionDto {
  name: string
  displayName: string
  description?: string
  group?: string
  isEnabled: boolean
}

/**
 * 更新权限DTO
 */
export interface UpdatePermissionDto {
  name: string
  displayName: string
  description?: string
  group?: string
  isEnabled: boolean
}

/**
 * 权限分组DTO
 */
export interface PermissionGroupDto {
  group: string
  permissions: PermissionDto[]
}

/**
 * 检查权限DTO
 */
export interface CheckPermissionDto {
  permissions: string[]
}

/**
 * 用户权限检查DTO
 */
export interface UserPermissionCheckDto {
  userId: string
  permission: string
  hasPermission: boolean
  source: string
}

/**
 * 用户权限信息DTO
 */
export interface UserPermissionInfoDto {
  userId: string
  username: string
  roles: RoleDto[]
  directPermissions: PermissionDto[]
  effectivePermissions: PermissionDto[]
}

/**
 * 分配用户权限DTO
 */
export interface AssignUserPermissionsDto {
  permissionIds: string[]
}

/**
 * @deprecated 使用 PermissionDto 替代
 */
export type Permission = PermissionDto

/**
 * @deprecated 使用 UserPermissionInfoDto 替代
 */
export type UserPermissionInfo = UserPermissionInfoDto
