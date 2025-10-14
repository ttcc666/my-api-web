# API 层使用指南

## 📁 目录结构

```
src/api/
├── auth.ts          # 认证相关 API
├── users.ts         # 用户管理 API
├── roles.ts         # 角色管理 API
├── permissions.ts   # 权限管理 API
├── token.ts         # Token 管理 API
├── interceptors.ts  # 请求/响应拦截器
└── index.ts         # 统一导出入口
```

## 📑 目录

- [快速开始](#快速开始)
- [API 速查表](#api-速查表)
- [详细 API 文档](#详细-api-文档)
  - [AuthApi - 认证相关](#1-authapi---认证相关)
  - [UsersApi - 用户管理](#2-usersapi---用户管理)
  - [RolesApi - 角色管理](#3-rolesapi---角色管理)
  - [PermissionsApi - 权限管理](#4-permissionsapi---权限管理)
  - [TokenApi - Token 管理](#5-tokenapi---token-管理)
- [完整使用示例](#完整使用示例)
- [最佳实践](#最佳实践)
- [常见问题](#常见问题)

## 🚀 快速开始

```typescript
import { UsersApi, RolesApi, AuthApi } from '@/api'

// 用户登录
const tokens = await UsersApi.login({ username: 'admin', password: '123456' })

// 获取当前用户信息
const user = await UsersApi.getProfile()

// 获取当前用户权限
const permissions = await AuthApi.getCurrentUserPermissions()

// 检查权限
const hasPermission = await AuthApi.checkCurrentUserPermission('user:edit')
```

## 📊 API 速查表

### 认证相关 (AuthApi)

| 方法                                     | 说明                 | 参数                 |
| ---------------------------------------- | -------------------- | -------------------- |
| `getCurrentUser()`                       | 获取当前用户信息     | -                    |
| `getCurrentUserPermissions()`            | 获取当前用户权限信息 | -                    |
| `checkCurrentUserPermission(permission)` | 检查当前用户权限     | `permission: string` |

### 用户管理 (UsersApi)

| 方法                             | 说明             | 参数                             |
| -------------------------------- | ---------------- | -------------------------------- |
| `register(data)`                 | 用户注册         | `UserRegisterDto`                |
| `login(data)`                    | 用户登录         | `UserLoginDto`                   |
| `getProfile()`                   | 获取当前用户资料 | -                                |
| `getAllUsers()`                  | 获取所有用户列表 | -                                |
| `getUserById(id)`                | 根据ID获取用户   | `id: string`                     |
| `updateUser(id, data)`           | 更新用户信息     | `id: string, UserUpdateDto`      |
| `deleteUser(id)`                 | 删除用户         | `id: string`                     |
| `getUserRoles(id)`               | 获取用户角色列表 | `id: string`                     |
| `assignUserRoles(id, data)`      | 分配用户角色     | `id: string, AssignUserRolesDto` |
| `removeUserRole(userId, roleId)` | 移除用户角色     | `userId: string, roleId: string` |

### 角色管理 (RolesApi)

| 方法                              | 说明                 | 参数                                   |
| --------------------------------- | -------------------- | -------------------------------------- |
| `getAllRoles()`                   | 获取所有角色列表     | -                                      |
| `getRoleById(id)`                 | 根据ID获取角色       | `id: string`                           |
| `createRole(data)`                | 创建新角色           | `CreateRoleDto`                        |
| `updateRole(id, data)`            | 更新角色信息         | `id: string, UpdateRoleDto`            |
| `deleteRole(id)`                  | 删除角色             | `id: string`                           |
| `getRolePermissions(id)`          | 获取角色权限列表     | `id: string`                           |
| `assignRolePermissions(id, data)` | 分配角色权限         | `id: string, AssignRolePermissionsDto` |
| `checkRoleName(name, excludeId?)` | 检查角色名称是否存在 | `name: string, excludeId?: string`     |

### 权限管理 (PermissionsApi)

| 方法                                         | 说明                 | 参数                                       |
| -------------------------------------------- | -------------------- | ------------------------------------------ |
| `getAllPermissions()`                        | 获取所有权限列表     | -                                          |
| `getPermissionById(id)`                      | 根据ID获取权限       | `id: string`                               |
| `createPermission(data)`                     | 创建新权限           | `CreatePermissionDto`                      |
| `updatePermission(id, data)`                 | 更新权限信息         | `id: string, UpdatePermissionDto`          |
| `deletePermission(id)`                       | 删除权限             | `id: string`                               |
| `getPermissionGroups()`                      | 获取权限分组列表     | -                                          |
| `checkPermissionName(name, excludeId?)`      | 检查权限名称是否存在 | `name: string, excludeId?: string`         |
| `getUserPermissions(userId)`                 | 获取用户权限信息     | `userId: string`                           |
| `checkUserPermission(userId, permission)`    | 检查用户权限         | `userId: string, permission: string`       |
| `checkUserPermissionsBatch(userId, data)`    | 批量检查用户权限     | `userId: string, CheckPermissionDto`       |
| `assignUserPermissions(userId, data)`        | 分配用户权限         | `userId: string, AssignUserPermissionsDto` |
| `removeUserPermission(userId, permissionId)` | 移除用户权限         | `userId: string, permissionId: string`     |
| `getUserDirectPermissions(userId)`           | 获取用户直接权限列表 | `userId: string`                           |

### Token 管理 (TokenApi)

| 方法                 | 说明                 | 参数                     |
| -------------------- | -------------------- | ------------------------ |
| `refreshToken(data)` | 刷新访问令牌         | `RefreshTokenRequestDto` |
| `logout(data)`       | 用户登出（撤销令牌） | `RefreshTokenRequestDto` |

## 📖 详细 API 文档

### 1. AuthApi - 认证相关

#### `getCurrentUser()`

获取当前登录用户的基本信息。

```typescript
import { AuthApi } from '@/api'

const currentUser = await AuthApi.getCurrentUser()
// 返回: { id, username, permissions, roles }
```

#### `getCurrentUserPermissions()`

获取当前用户的完整权限信息，包括角色权限和直接权限。

```typescript
const permissionInfo = await AuthApi.getCurrentUserPermissions()
// 返回: { userId, username, roles, directPermissions, effectivePermissions }
```

#### `checkCurrentUserPermission(permission)`

检查当前用户是否拥有指定权限。

```typescript
const result = await AuthApi.checkCurrentUserPermission('user:edit')
// 返回: { userId, permission, hasPermission, source }
```

### 2. UsersApi - 用户管理

#### `register(registerData)`

用户注册。

```typescript
import { UsersApi } from '@/api'

const user = await UsersApi.register({
  username: 'newuser',
  email: 'user@example.com',
  password: 'password123',
  realName: '张三',
  phone: '13800138000',
})
```

#### `login(loginData)`

用户登录，返回访问令牌和刷新令牌。

```typescript
const tokens = await UsersApi.login({
  username: 'admin',
  password: 'password123',
})
// 返回: { accessToken, refreshToken }
```

#### `getProfile()`

获取当前登录用户的详细资料。

```typescript
const profile = await UsersApi.getProfile()
// 返回: { id, username, email, realName, phone, isActive, createdTime, lastLoginTime }
```

#### `getAllUsers()`

获取所有用户列表（需要相应权限）。

```typescript
const users = await UsersApi.getAllUsers()
// 返回: UserDto[]
```

#### `getUserById(id)`

根据用户ID获取用户详细信息。

```typescript
const user = await UsersApi.getUserById('user-id-123')
```

#### `updateUser(id, updateData)`

更新用户信息。

```typescript
await UsersApi.updateUser('user-id-123', {
  realName: '李四',
  phone: '13900139000',
})
```

#### `deleteUser(id)`

删除指定用户。

```typescript
await UsersApi.deleteUser('user-id-123')
```

#### `getUserRoles(id)`

获取用户的角色列表。

```typescript
const roles = await UsersApi.getUserRoles('user-id-123')
// 返回: RoleDto[]
```

#### `assignUserRoles(id, rolesData)`

为用户分配角色。

```typescript
await UsersApi.assignUserRoles('user-id-123', {
  roleIds: ['role-1', 'role-2'],
})
```

#### `removeUserRole(userId, roleId)`

移除用户的指定角色。

```typescript
await UsersApi.removeUserRole('user-id-123', 'role-1')
```

### 3. RolesApi - 角色管理

#### `getAllRoles()`

获取所有角色列表。

```typescript
import { RolesApi } from '@/api'

const roles = await RolesApi.getAllRoles()
// 返回: RoleDto[]
```

#### `getRoleById(id)`

根据角色ID获取角色详细信息。

```typescript
const role = await RolesApi.getRoleById('role-id-123')
```

#### `createRole(roleData)`

创建新角色。

```typescript
const role = await RolesApi.createRole({
  name: 'Editor',
  description: '编辑者',
  isEnabled: true,
  permissionIds: ['perm-1', 'perm-2'],
})
```

#### `updateRole(id, roleData)`

更新角色信息（不包括权限）。

```typescript
await RolesApi.updateRole('role-id-123', {
  name: 'Senior Editor',
  description: '高级编辑者',
  isEnabled: true,
})
```

#### `deleteRole(id)`

删除指定角色。

```typescript
await RolesApi.deleteRole('role-id-123')
```

#### `getRolePermissions(id)`

获取角色的权限列表。

```typescript
const permissions = await RolesApi.getRolePermissions('role-id-123')
// 返回: PermissionDto[]
```

#### `assignRolePermissions(id, permissionsData)`

为角色分配权限。

```typescript
await RolesApi.assignRolePermissions('role-id-123', {
  permissionIds: ['perm-1', 'perm-2', 'perm-3'],
})
```

#### `checkRoleName(name, excludeId?)`

检查角色名称是否已存在（用于表单验证）。

```typescript
const result = await RolesApi.checkRoleName('Admin', 'current-role-id')
// 返回: { exists: boolean }
```

### 4. PermissionsApi - 权限管理

#### `getAllPermissions()`

获取所有权限列表。

```typescript
import { PermissionsApi } from '@/api'

const permissions = await PermissionsApi.getAllPermissions()
// 返回: PermissionDto[]
```

#### `getPermissionById(id)`

根据权限ID获取权限详细信息。

```typescript
const permission = await PermissionsApi.getPermissionById('perm-id-123')
```

#### `createPermission(permissionData)`

创建新权限。

```typescript
const permission = await PermissionsApi.createPermission({
  name: 'user:create',
  displayName: '创建用户',
  description: '允许创建新用户',
  group: 'user',
  isEnabled: true,
})
```

#### `updatePermission(id, permissionData)`

更新权限信息。

```typescript
await PermissionsApi.updatePermission('perm-id-123', {
  name: 'user:create',
  displayName: '创建用户',
  description: '允许创建新用户账号',
  group: 'user',
  isEnabled: true,
})
```

#### `deletePermission(id)`

删除指定权限。

```typescript
await PermissionsApi.deletePermission('perm-id-123')
```

#### `getPermissionGroups()`

获取权限分组列表（按组分类的权限）。

```typescript
const groups = await PermissionsApi.getPermissionGroups()
// 返回: PermissionGroupDto[] - [{ group: 'user', permissions: [...] }, ...]
```

#### `checkPermissionName(name, excludeId?)`

检查权限名称是否已存在（用于表单验证）。

```typescript
const result = await PermissionsApi.checkPermissionName('user:create', 'current-perm-id')
// 返回: { exists: boolean }
```

#### `getUserPermissions(userId)`

获取指定用户的权限信息。

```typescript
const userPerms = await PermissionsApi.getUserPermissions('user-id-123')
// 返回: { userId, username, roles, directPermissions, effectivePermissions }
```

#### `checkUserPermission(userId, permission)`

检查指定用户是否拥有某个权限。

```typescript
const result = await PermissionsApi.checkUserPermission('user-id-123', 'user:edit')
// 返回: { userId, permission, hasPermission, source }
```

#### `checkUserPermissionsBatch(userId, permissions)`

批量检查用户权限。

```typescript
const results = await PermissionsApi.checkUserPermissionsBatch('user-id-123', {
  permissions: ['user:view', 'user:edit', 'user:delete'],
})
// 返回: UserPermissionCheckDto[]
```

#### `assignUserPermissions(userId, permissionsData)`

为用户分配直接权限。

```typescript
await PermissionsApi.assignUserPermissions('user-id-123', {
  permissionIds: ['perm-1', 'perm-2'],
})
```

#### `removeUserPermission(userId, permissionId)`

移除用户的指定权限。

```typescript
await PermissionsApi.removeUserPermission('user-id-123', 'perm-1')
```

#### `getUserDirectPermissions(userId)`

获取用户的直接权限列表（不包括角色权限）。

```typescript
const directPerms = await PermissionsApi.getUserDirectPermissions('user-id-123')
// 返回: PermissionDto[]
```

### 5. TokenApi - Token 管理

#### `refreshToken(refreshTokenData)`

使用刷新令牌获取新的访问令牌。

```typescript
import { TokenApi } from '@/api'

const newTokens = await TokenApi.refreshToken({
  refreshToken: 'your-refresh-token',
})
// 返回: { accessToken, refreshToken }
```

#### `logout(refreshTokenData)`

用户登出，撤销刷新令牌。

```typescript
await TokenApi.logout({
  refreshToken: 'your-refresh-token',
})
```

## 💡 完整使用示例

### 用户管理完整流程

```typescript
import { UsersApi, RolesApi } from '@/api'
import { useMessage } from 'naive-ui'

const message = useMessage()

// 1. 获取所有用户
const users = await UsersApi.getAllUsers()

// 2. 创建新用户（通过注册）
const newUser = await UsersApi.register({
  username: 'newuser',
  email: 'newuser@example.com',
  password: 'password123',
  realName: '新用户',
})

// 3. 更新用户信息
await UsersApi.updateUser(newUser.id, {
  realName: '更新后的名字',
  phone: '13800138000',
})

// 4. 为用户分配角色
const roles = await RolesApi.getAllRoles()
await UsersApi.assignUserRoles(newUser.id, {
  roleIds: [roles[0].id],
})

// 5. 查看用户角色
const userRoles = await UsersApi.getUserRoles(newUser.id)
console.log('用户角色:', userRoles)

// 6. 删除用户
await UsersApi.deleteUser(newUser.id)
message.success('用户删除成功')
```

### 角色权限管理流程

```typescript
import { RolesApi, PermissionsApi } from '@/api'

// 1. 获取所有权限（按组分类）
const permissionGroups = await PermissionsApi.getPermissionGroups()

// 2. 创建新角色并分配权限
const role = await RolesApi.createRole({
  name: 'Content Manager',
  description: '内容管理员',
  isEnabled: true,
  permissionIds: ['perm-1', 'perm-2'],
})

// 3. 更新角色基本信息
await RolesApi.updateRole(role.id, {
  name: 'Senior Content Manager',
  description: '高级内容管理员',
  isEnabled: true,
})

// 4. 更新角色权限
await RolesApi.assignRolePermissions(role.id, {
  permissionIds: ['perm-1', 'perm-2', 'perm-3'],
})

// 5. 查看角色权限
const rolePermissions = await RolesApi.getRolePermissions(role.id)
console.log('角色权限:', rolePermissions)
```

### 权限检查流程

```typescript
import { AuthApi, PermissionsApi } from '@/api'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()

// 方式1: 使用 AuthApi 检查当前用户权限
const canEdit = await AuthApi.checkCurrentUserPermission('user:edit')
if (canEdit.hasPermission) {
  // 执行编辑操作
}

// 方式2: 使用 Store 中的权限检查（推荐，无需网络请求）
if (authStore.hasPermission('user:edit')) {
  // 执行编辑操作
}

// 方式3: 批量检查权限
const results = await PermissionsApi.checkUserPermissionsBatch(userId, {
  permissions: ['user:view', 'user:edit', 'user:delete'],
})
results.forEach((result) => {
  console.log(`${result.permission}: ${result.hasPermission}`)
})
```

### 与 Composables 配合使用

```typescript
import { useUserManagement } from '@/composables/useUserManagement'
import { useRoleManagement } from '@/composables/useRoleManagement'

// 用户管理
const { users, loading, loadUsers, createUser, updateUser, deleteUser } = useUserManagement()

// 加载用户列表
await loadUsers()

// 创建用户
await createUser({
  username: 'newuser',
  email: 'user@example.com',
  password: 'password123',
})

// 角色管理
const { roles, permissions, loadRoles, loadPermissions, createRole, updateRole } =
  useRoleManagement()

await loadRoles()
await loadPermissions()
```

## 🎯 最佳实践

### 1. 统一导入

从 `@/api` 统一导入，而不是直接导入具体文件：

```typescript
// ✅ 推荐
import { UsersApi, RolesApi, AuthApi } from '@/api'

// ❌ 不推荐
import UsersApi from '@/api/users'
import RolesApi from '@/api/roles'
```

### 2. 类型安全

使用导出的 TypeScript 类型：

```typescript
import { UsersApi, type UserDto, type UserLoginDto } from '@/api'

const loginData: UserLoginDto = {
  username: 'admin',
  password: 'password123',
}

const user: UserDto = await UsersApi.getProfile()
```

### 3. 错误处理

API 调用会自动通过拦截器处理错误，但仍需处理业务逻辑：

```typescript
import { useMessage } from 'naive-ui'

const message = useMessage()

try {
  await UsersApi.login(loginData)
  message.success('登录成功')
  // 跳转到首页
} catch (error) {
  // 错误已被拦截器处理并显示
  // 这里只需处理业务逻辑，如清理表单
  console.error('登录失败:', error)
}
```

### 4. 权限检查优先级

```typescript
import { useAuthStore } from '@/stores/auth'
import { AuthApi } from '@/api'

const authStore = useAuthStore()

// 优先使用 Store 中的权限检查（无需网络请求）
if (authStore.hasPermission('user:edit')) {
  // 执行操作
}

// 仅在需要实时验证时使用 API
const result = await AuthApi.checkCurrentUserPermission('user:edit')
```

### 5. 表单验证

使用 `checkRoleName` 和 `checkPermissionName` 进行异步验证：

```typescript
import { RolesApi } from '@/api'

const roleNameRule = {
  async validator(rule: any, value: string) {
    if (!value) return true

    const result = await RolesApi.checkRoleName(value, editingRoleId)
    if (result.exists) {
      throw new Error('角色名称已存在')
    }
    return true
  },
  trigger: 'blur',
}
```

### 6. 性能优化

```typescript
// 使用 Promise.all 并行请求
const [users, roles, permissions] = await Promise.all([
  UsersApi.getAllUsers(),
  RolesApi.getAllRoles(),
  PermissionsApi.getAllPermissions(),
])

// 使用权限分组减少请求次数
const permissionGroups = await PermissionsApi.getPermissionGroups()
// 而不是多次调用 getAllPermissions()
```

### 7. Token 刷新

Token 刷新由拦截器自动处理，无需手动调用：

```typescript
// ❌ 不需要手动刷新
// await TokenApi.refreshToken({ refreshToken })

// ✅ 拦截器会自动处理
// 只需正常调用 API，拦截器会在 token 即将过期时自动刷新
const user = await UsersApi.getProfile()
```

## ❓ 常见问题

### Q1: 为什么有些 API 方法返回 void？

A: 对于删除、更新等操作，后端返回成功状态即可，不需要返回数据。拦截器会自动处理响应。

```typescript
// 返回 void，但操作成功
await UsersApi.deleteUser(userId)
```

### Q2: 如何处理 401 未授权错误？

A: 拦截器会自动处理 401 错误：

- 如果是 token 过期，会自动刷新 token 并重试请求
- 如果刷新失败，会自动登出并跳转到登录页

### Q3: 如何在组件中使用 API？

A: 推荐使用 Composables 封装 API 调用：

```typescript
// 在组件中
import { useUserManagement } from '@/composables/useUserManagement'

const { users, loading, loadUsers } = useUserManagement()

onMounted(() => {
  loadUsers()
})
```

### Q4: 权限检查应该用 API 还是 Store？

A: 优先使用 Store：

- **Store**: 适用于 UI 显示控制（按钮、菜单等）
- **API**: 适用于需要实时验证的场景（如敏感操作前的二次确认）

### Q5: 如何处理并发请求？

A: 使用 `Promise.all` 或 `Promise.allSettled`：

```typescript
// 并行请求，全部成功才继续
const [users, roles] = await Promise.all([UsersApi.getAllUsers(), RolesApi.getAllRoles()])

// 并行请求，部分失败也继续
const results = await Promise.allSettled([UsersApi.getAllUsers(), RolesApi.getAllRoles()])
```

### Q6: 如何自定义请求头？

A: 拦截器会自动添加 Authorization 头，如需自定义其他头：

```typescript
import apiClient from '@/utils/request'

// 直接使用 apiClient 并自定义配置
const response = await apiClient.get('/custom-endpoint', {
  headers: {
    'X-Custom-Header': 'value',
  },
})
```

### Q7: Token 什么时候会自动刷新？

A: 拦截器会在以下情况自动刷新 token：

- Token 即将过期（30秒内）
- 收到 401 响应且响应头包含 `token-expired: true`

### Q8: 如何处理文件上传？

A: 使用 FormData 并设置正确的 Content-Type：

```typescript
import apiClient from '@/utils/request'

const formData = new FormData()
formData.append('file', file)

await apiClient.post('/upload', formData, {
  headers: {
    'Content-Type': 'multipart/form-data',
  },
})
```

## 🔧 拦截器说明

所有 API 请求都会经过以下拦截器处理：

### 请求拦截器

- 自动添加 `Authorization` 头
- 检查 token 是否即将过期，自动刷新
- 处理 token 刷新队列，避免并发刷新

### 响应拦截器

- 统一错误处理和消息提示
- 自动解包响应数据
- 处理 401 错误和 token 过期
- 自动重试失败的请求

详见 [`interceptors.ts`](./interceptors.ts)
