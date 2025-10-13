// API响应类型定义
export interface ApiResponse<T = any> {
  data?: T
  message: string
  token?: string
  total?: number
}

// 用户相关类型
export interface User {
  id: number
  username: string
  email: string
  realName?: string
  phone?: string
  isActive: boolean
  createdTime: string
  lastLoginTime?: string
}

export interface UserRegisterDto {
  username: string
  email: string
  password: string
  realName?: string
  phone?: string
}

export interface UserLoginDto {
  username: string
  password: string
}

export interface UserUpdateDto {
  realName?: string
  phone?: string
}

// API错误类型
export interface ApiError {
  message: string
  status?: number
}

// 权限和角色相关类型
export interface Permission {
  id: string
  name: string
  displayName: string
  description?: string
  group?: string
  isEnabled: boolean
  creationTime: string
}

export interface Role {
  id: string
  name: string
  description?: string
  isSystem: boolean
  isEnabled: boolean
  creationTime: string
  permissions: Permission[]
}

export interface UserPermissionInfo {
  userId: string
  username: string
  roles: Role[]
  directPermissions: Permission[]
  effectivePermissions: Permission[]
}