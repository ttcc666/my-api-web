/**
 * API 服务统一入口
 *
 * API 类（按资源组织）：
 * - AuthApi: 认证相关（登录、权限检查等）
 * - UsersApi: 用户管理
 * - RolesApi: 角色管理
 * - PermissionsApi: 权限管理
 * - TokenApi: Token 管理
 * - DeviceApi: 设备信息
 * - OnlineUsersApi: 在线用户管理
 */

export { AuthApi, default as AuthApiDefault } from './auth'
export { UsersApi, default as UsersApiDefault } from './users'
export { TokenApi, default as TokenApiDefault } from './token'
export { RolesApi, default as RolesApiDefault } from './roles'
export { PermissionsApi, default as PermissionsApiDefault } from './permissions'
export { MenusApi, default as MenusApiDefault } from './menus'
export { DeviceApi, default as DeviceApiDefault } from './device'
export { OnlineUsersApi, default as OnlineUsersApiDefault } from './onlineUsers'

// 重新导出类型
export type * from '@/types/api'
