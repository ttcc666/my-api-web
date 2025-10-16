// ===================================
// General API Response
// ===================================

/**
 * API响应的通用结构
 */
export interface ApiResponse<T = unknown> {
  success: boolean
  code: number
  message: string
  data: T | null
  total?: number // 用于分页
}

/**
 * API错误详情
 */
export interface ApiError {
  message: string
  status?: number
}

/**
 * 检查资源是否存在响应的DTO
 */
export interface ExistsResponseDto {
  exists: boolean
}

// ===================================
// User Related Types
// ===================================

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
 * 用户登录DTO
 */
export interface UserLoginDto {
  username: string
  password: string
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

// ===================================
// Token Related Types
// ===================================

/**
 * Token响应DTO
 */
export interface TokenDto {
  accessToken: string
  refreshToken: string
}

/**
 * 刷新Token请求DTO
 */
export interface RefreshTokenRequestDto {
  refreshToken: string
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

// ===================================
// Permission Related Types
// ===================================

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

// ===================================
// Menu Related Types
// ===================================

/**
 * 菜单类型
 */
export enum MenuType {
  Directory = 0,
  Route = 1,
}

/**
 * 菜单信息 DTO
 */
export interface MenuDto {
  id: string
  code: string
  title: string
  routePath?: string | null
  routeName?: string | null
  icon?: string | null
  parentId?: string | null
  order: number
  isEnabled: boolean
  type: MenuType
  permissionCode?: string | null
  description?: string | null
  children?: MenuDto[]
}

/**
 * 创建菜单 DTO
 */
export interface CreateMenuDto {
  code: string
  title: string
  routePath?: string | null
  routeName?: string | null
  icon?: string | null
  parentId?: string | null
  order: number
  isEnabled: boolean
  type: MenuType
  permissionCode?: string | null
  description?: string | null
}

/**
 * 更新菜单 DTO
 */
export interface UpdateMenuDto {
  title: string
  routePath?: string | null
  routeName?: string | null
  icon?: string | null
  parentId?: string | null
  order: number
  isEnabled: boolean
  type: MenuType
  permissionCode?: string | null
  description?: string | null
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

// ===================================
// Role Related Types
// ===================================

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

// ===================================
// Legacy Type Aliases (for backward compatibility)
// ===================================

/**
 * @deprecated 使用 UserDto 替代
 */
export type User = UserDto

/**
 * @deprecated 使用 PermissionDto 替代
 */
export type Permission = PermissionDto

/**
 * @deprecated 使用 RoleDto 替代
 */
export type Role = RoleDto

/**
 * @deprecated 使用 TokenDto 替代
 */
export type TokenPayload = TokenDto

/**
 * @deprecated 使用 UserPermissionInfoDto 替代
 */
export type UserPermissionInfo = UserPermissionInfoDto

// ===================================
// Online User Related Types
// ===================================

/**
 * 在线用户状态枚举
 */
export enum OnlineUserStatus {
  Online = 'Online',
  Idle = 'Idle',
  Offline = 'Offline',
}

/**
 * 在线用户信息 DTO
 */
export interface OnlineUserDto {
  id: string
  connectionId: string
  userId: string
  username?: string | null
  connectedAt: string
  lastHeartbeatAt: string
  ipAddress?: string | null
  userAgent?: string | null
  room?: string | null
  status: OnlineUserStatus
  disconnectedAt?: string | null
  onlineDurationSeconds?: number
}

/**
 * 在线用户统计信息 DTO
 */
export interface OnlineUserStatisticsDto {
  totalOnlineUsers: number
  totalConnections: number
  todayPeakUsers: number
  activeUsers: number
  idleUsers: number
  averageOnlineDurationSeconds: number
  statisticsTime: string
}

/**
 * 在线用户查询参数
 */
export interface OnlineUserQueryParams {
  pageNumber?: number
  pageSize?: number
  status?: OnlineUserStatus
  userId?: string
  room?: string
}

/**
 * 在线用户列表响应 DTO
 */
export interface OnlineUserListResponseDto {
  users: OnlineUserDto[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
}

/**
 * 在线用户数量响应 DTO
 */
export interface OnlineUserCountResponseDto {
  count: number
}
