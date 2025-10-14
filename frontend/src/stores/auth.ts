import { ref, computed, readonly } from 'vue'
import { defineStore } from 'pinia'
import type { UserLoginDto, UserRegisterDto } from '@/types/api'
import { UsersApi } from '@/api'
import { useUserStore } from './user'
import { usePermissionStore } from './permission'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem('accessToken'))
  const refreshToken = ref<string | null>(localStorage.getItem('refreshToken'))
  const tokenExpiresAt = ref<number | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  const isAuthenticated = computed(() => !!token.value)

  const clearError = () => {
    error.value = null
  }

  const setTokens = (accessToken: string, newRefreshToken: string) => {
    token.value = accessToken
    refreshToken.value = newRefreshToken
    tokenExpiresAt.value = parseTokenExpiry(accessToken)

    localStorage.setItem('accessToken', accessToken)
    localStorage.setItem('refreshToken', newRefreshToken)
  }

  const login = async (loginData: UserLoginDto): Promise<boolean> => {
    try {
      loading.value = true
      error.value = null

      const tokens = await UsersApi.login(loginData)
      setTokens(tokens.accessToken, tokens.refreshToken)

      const userStore = useUserStore()
      const permissionStore = usePermissionStore()

      const userData = await UsersApi.getProfile()
      userStore.setUser(userData)

      await permissionStore.loadUserPermissions()

      return true
    } catch (err: unknown) {
      error.value = (err as Error).message || '登录失败'
      return false
    } finally {
      loading.value = false
    }
  }

  const register = async (registerData: UserRegisterDto): Promise<boolean> => {
    try {
      loading.value = true
      error.value = null

      await UsersApi.register(registerData)
      return true
    } catch (err: unknown) {
      error.value = (err as Error).message || '注册失败'
      return false
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

  const logout = () => {
    token.value = null
    refreshToken.value = null
    tokenExpiresAt.value = null

    const userStore = useUserStore()
    const permissionStore = usePermissionStore()

    userStore.clearUser()
    permissionStore.clearUserPermissions()

    localStorage.clear()
  }

  const initializeAuth = async () => {
    const storedToken = localStorage.getItem('accessToken')
    const storedRefreshToken = localStorage.getItem('refreshToken')

    if (storedToken && storedRefreshToken) {
      token.value = storedToken
      refreshToken.value = storedRefreshToken
      tokenExpiresAt.value = parseTokenExpiry(storedToken)

      const userStore = useUserStore()
      const permissionStore = usePermissionStore()

      userStore.initializeUser()

      if (!userStore.user) {
        await userStore.fetchUserInfo()
      }

      await permissionStore.loadUserPermissions()
    }
  }

  return {
    token: readonly(token),
    refreshToken: readonly(refreshToken),
    tokenExpiresAt: readonly(tokenExpiresAt),
    loading: readonly(loading),
    error: readonly(error),
    isAuthenticated,
    clearError,
    setTokens,
    login,
    register,
    logout,
    shouldRefreshToken,
    isAccessTokenExpired,
    initializeAuth,
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
