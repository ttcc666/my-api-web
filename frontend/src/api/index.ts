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

export { AuthApi, default as AuthApiDefault } from './modules/auth/auth'
export { UsersApi, default as UsersApiDefault } from './modules/system/users'
export { TokenApi, default as TokenApiDefault } from './modules/auth/token'
export { RolesApi, default as RolesApiDefault } from './modules/system/roles'
export { PermissionsApi, default as PermissionsApiDefault } from './modules/system/permissions'
export { MenusApi, default as MenusApiDefault } from './modules/system/menus'
export { DeviceApi, default as DeviceApiDefault } from './modules/system/device'
export { OnlineUsersApi, default as OnlineUsersApiDefault } from './modules/hub/onlineUsers'
export { captchaApi } from './modules/common/captcha'

// 重新导出类型
export type * from '@/types/api'
