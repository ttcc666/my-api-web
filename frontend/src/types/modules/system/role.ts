/**
 * 角色相关类型定义
 * Role Related Type Definitions
 */

import type { PermissionDto } from './permission'

/**
 * 角色信息
 */
export interface RoleDto {
  id: string
  name: string
  description?: string
  isSystem: boolean
  isEnabled: boolean
  creationTime: string
  permissions?: PermissionDto[]
}

/**
 * 创建角色DTO
 */
export interface CreateRoleDto {
  name: string
  description?: string
  isEnabled: boolean
  permissionIds?: string[]
}

/**
 * 更新角色DTO
 */
export interface UpdateRoleDto {
  name: string
  description?: string
  isEnabled: boolean
}

/**
 * 分配角色权限DTO
 */
export interface AssignRolePermissionsDto {
  permissionIds: string[]
}

/**
 * @deprecated 使用 RoleDto 替代
 */
export type Role = RoleDto
