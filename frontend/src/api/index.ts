// 统一的 API 服务入口文件
export { AuthApi, default as AuthApiDefault } from './auth'
export { UsersApi, default as UsersApiDefault } from './users'
export { TokenApi, default as TokenApiDefault } from './token'
export { RolesApi, default as RolesApiDefault } from './roles'
export { PermissionsApi, default as PermissionsApiDefault } from './permissions'

// 重新导出类型
export type * from '@/types/api'
