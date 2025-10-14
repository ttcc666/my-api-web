import { ref, computed, readonly } from 'vue'
import { defineStore } from 'pinia'
import type { UserDto, UserLoginDto, UserRegisterDto, UserPermissionInfoDto } from '@/types/api'

// 类型别名以保持兼容性
type User = UserDto
type UserPermissionInfo = UserPermissionInfoDto
import UserApi from '@/api/user'

export const useAuthStore = defineStore('auth', () => {
  // 状态
  const user = ref<User | null>(null)
  const token = ref<string | null>(localStorage.getItem('accessToken'))
  const refreshToken = ref<string | null>(localStorage.getItem('refreshToken'))
  const tokenExpiresAt = ref<number | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  // 权限相关状态
  const userPermissions = ref<UserPermissionInfo | null>(null)
  const permissions = ref<string[]>([])
  const roles = ref<string[]>([])
  const permissionsLoaded = ref(false) // 增加一个状态来标记权限是否已加载

  // 计算属性
  const isAuthenticated = computed(() => !!token.value)
  const username = computed(() => user.value?.username || '')

  // 权限检查计算属性
  const hasPermission = computed(() => (permission: string) => {
    return permissions.value.includes(permission)
  })

  const hasRole = computed(() => (role: string) => {
    return roles.value.includes(role)
  })

  const hasAnyPermission = computed(() => (permissionList: string[]) => {
    return permissionList.some((permission) => permissions.value.includes(permission))
  })

  const hasAllPermissions = computed(() => (permissionList: string[]) => {
    return permissionList.every((permission) => permissions.value.includes(permission))
  })

  // 清除错误
  const clearError = () => {
    error.value = null
  }

  // 设置用户信息
  const setUser = (userData: User) => {
    user.value = userData
    localStorage.setItem('user', JSON.stringify(userData))
  }

  // 设置双令牌
  const setTokens = (accessToken: string, newRefreshToken: string) => {
    token.value = accessToken
    refreshToken.value = newRefreshToken
    tokenExpiresAt.value = parseTokenExpiry(accessToken)

    localStorage.setItem('accessToken', accessToken)
    localStorage.setItem('refreshToken', newRefreshToken)
  }

  // 登录
  const login = async (loginData: UserLoginDto): Promise<boolean> => {
    try {
      loading.value = true
      error.value = null

      const tokens = await UserApi.login(loginData)
      setTokens(tokens.accessToken, tokens.refreshToken)

      // 确保在下一次请求之前，新的 apiClient 实例能够获取到 token
      // 实际上，由于 apiClient 是单例，这个问题不应该发生，但为了保险起见，我们可以在这里重新创建一个实例
      // 或者，更简单的方式是，确保 getProfile 内部的请求能够正确获取到 token
      // 让我们假设 apiClient 的实现是正确的，问题可能出在其他地方

      // 获取用户信息
      const userData = await UserApi.getProfile()
      setUser(userData)

      // 登录成功后加载用户权限
      await loadUserPermissions()

      return true
    } catch (err: unknown) {
      error.value = (err as Error).message || '登录失败'
      return false
    } finally {
      loading.value = false
    }
  }

  // 注册
  const register = async (registerData: UserRegisterDto): Promise<boolean> => {
    try {
      loading.value = true
      error.value = null

      await UserApi.register(registerData)
      return true
    } catch (err: unknown) {
      error.value = (err as Error).message || '注册失败'
      return false
    } finally {
      loading.value = false
    }
  }

  // 权限相关方法
  const setUserPermissions = (permissionInfo: UserPermissionInfo) => {
    if (!permissionInfo) return
    userPermissions.value = permissionInfo
    permissions.value = (permissionInfo.effectivePermissions || []).map((p) => p.name)
    roles.value = (permissionInfo.roles || []).map((r) => r.name)
  }

  const clearUserPermissions = () => {
    userPermissions.value = null
    permissions.value = []
    roles.value = []
    permissionsLoaded.value = false // 清除权限时重置状态
  }

  const loadUserPermissions = async () => {
    // 如果权限已加载或用户未认证，则直接返回，避免重复请求
    if (permissionsLoaded.value || !isAuthenticated.value) return

    try {
      loading.value = true
      const permissionInfo = await UserApi.getUserPermissions()
      if (permissionInfo) {
        setUserPermissions(permissionInfo)
        permissionsLoaded.value = true // 标记权限已成功加载
      }
    } catch (err) {
      console.error('加载用户权限失败:', err)
      error.value = '加载用户权限失败'
      permissionsLoaded.value = false // 加载失败时也应重置状态
      throw err // 重新抛出错误，以便路由守卫可以捕获
    } finally {
      loading.value = false
    }
  }

  const shouldRefreshToken = (bufferMs = 30_000) => {
    if (!token.value || !tokenExpiresAt.value) return false
    return tokenExpiresAt.value - bufferMs <= Date.now()
  }

  const isAccessTokenExpired = () => {
    if (!token.value || !tokenExpiresAt.value) return true
    return tokenExpiresAt.value <= Date.now()
  }

  // 登出
  const logout = () => {
    user.value = null
    token.value = null
    refreshToken.value = null
    tokenExpiresAt.value = null
    clearUserPermissions()

    // 清除所有本地存储，确保完全登出
    localStorage.clear()

    // 可以在这里调用后端的 logout 接口，使其 RefreshToken 失效
    // UserApi.logout({ refreshToken: refreshToken.value })
  }

  // 获取用户信息
  const fetchUserInfo = async (): Promise<boolean> => {
    try {
      if (!token.value) {
        return false
      }

      loading.value = true
      const userData = await UserApi.getProfile()
      setUser(userData)
      return true
    } catch (err: unknown) {
      error.value = (err as Error).message || '获取用户信息失败'
      // 如果获取用户信息失败，清除本地存储
      logout()
      return false
    } finally {
      loading.value = false
    }
  }

  // 初始化认证状态
  const initializeAuth = async () => {
    const storedToken = localStorage.getItem('accessToken')
    const storedRefreshToken = localStorage.getItem('refreshToken')
    const storedUser = localStorage.getItem('user')

    if (storedToken && storedRefreshToken) {
      token.value = storedToken
      refreshToken.value = storedRefreshToken
      tokenExpiresAt.value = parseTokenExpiry(storedToken)

      if (storedUser) {
        try {
          user.value = JSON.parse(storedUser)
        } catch {
          // 如果解析失败，重新获取用户信息
          await fetchUserInfo()
        }
      } else {
        // 没有用户信息，重新获取
        await fetchUserInfo()
      }
      // 初始化时也加载用户权限
      await loadUserPermissions()
    }
  }

  return {
    // 状态
    user: readonly(user),
    token: readonly(token),
    refreshToken: readonly(refreshToken),
    tokenExpiresAt: readonly(tokenExpiresAt),
    loading: readonly(loading),
    error: readonly(error),
    userPermissions: readonly(userPermissions),
    permissions: readonly(permissions),
    roles: readonly(roles),
    permissionsLoaded: readonly(permissionsLoaded),
    // 计算属性
    isAuthenticated,
    username,
    hasPermission,
    hasRole,
    hasAnyPermission,
    hasAllPermissions,
    shouldRefreshToken,
    isAccessTokenExpired,
    // 方法
    clearError,
    setUser,
    setTokens,
    login,
    register,
    logout,
    fetchUserInfo,
    initializeAuth,
    setUserPermissions,
    clearUserPermissions,
    loadUserPermissions,
  }
})

function parseTokenExpiry(accessToken: string): number | null {
  try {
    const payloadSegment = accessToken.split('.')[1]
    if (!payloadSegment) return null
    const payload = JSON.parse(atob(padBase64(payloadSegment))) as { exp?: number }
    if (typeof payload.exp !== 'number') return null
    return payload.exp * 1000
  } catch (error) {
    console.warn('解析 AccessToken 失败，无法获取过期时间', error)
    return null
  }
}

function padBase64(value: string): string {
  const base64 = value.replace(/-/g, '+').replace(/_/g, '/')
  const padLength = base64.length % 4
  if (padLength === 0) {
    return base64
  }
  return base64 + '='.repeat(4 - padLength)
}
