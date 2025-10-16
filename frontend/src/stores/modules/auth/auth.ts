import { ref, computed } from 'vue'
import { defineStore, storeToRefs } from 'pinia'
import type { UserLoginDto, UserRegisterDto } from '@/types/api'
import { UsersApi } from '@/api'
import { useUserStore } from '@/stores/modules/system/user'
import { usePermissionStore } from '@/stores/modules/system/permission'
import { useMenuStore } from '@/stores/modules/system/menu'
import { useOnlineUserStore } from '@/stores/modules/hub/onlineUser'
import { CacheManager } from '@/utils/cache'

export const useAuthStore = defineStore(
  'auth',
  () => {
    const token = ref<string | null>(null)
    const refreshToken = ref<string | null>(null)
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
    }

    const login = async (loginData: UserLoginDto): Promise<boolean> => {
      try {
        loading.value = true
        error.value = null

        CacheManager.clear()

        const tokens = await UsersApi.login(loginData)
        setTokens(tokens.accessToken, tokens.refreshToken)

        const userStore = useUserStore()
        const permissionStore = usePermissionStore()
        const menuStore = useMenuStore()
        const onlineUserStore = useOnlineUserStore()

        const userData = await UsersApi.getProfile()
        userStore.setUser(userData)

        await permissionStore.loadUserPermissions(true)

        try {
          await menuStore.refreshMenus()
        } catch (err) {
          console.error('加载菜单失败:', err)
        }

        // 登录成功后建立 SignalR 连接
        try {
          await onlineUserStore.initConnection(() => token.value || '')
          console.log('SignalR 连接已建立')
        } catch (err) {
          console.error('建立 SignalR 连接失败:', err)
          // 不影响登录流程,继续
        }

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
      const menuStore = useMenuStore()
      const onlineUserStore = useOnlineUserStore()

      userStore.clearUser()
      permissionStore.clearUserPermissions()
      menuStore.clearMenus()

      // 断开 SignalR 连接
      onlineUserStore.disconnect().catch((err) => {
        console.error('断开 SignalR 连接失败:', err)
      })

      CacheManager.clear()
    }

    const initializeAuth = async () => {
      if (!token.value || !refreshToken.value) {
        return
      }

      if (!tokenExpiresAt.value) {
        tokenExpiresAt.value = parseTokenExpiry(token.value)
      }

      const userStore = useUserStore()
      const permissionStore = usePermissionStore()
      const menuStore = useMenuStore()
      const onlineUserStore = useOnlineUserStore()
      const { user } = storeToRefs(userStore)

      if (!user.value) {
        await userStore.fetchUserInfo()
      }

      await permissionStore.loadUserPermissions()

      try {
        await menuStore.loadMenus()
      } catch (err) {
        console.error('初始化菜单失败:', err)
      }

      // 建立 SignalR 连接(用于自动登录场景)
      try {
        await onlineUserStore.initConnection(() => token.value || '')
        console.log('[Auth] 自动登录后 SignalR 连接已建立')
      } catch (err) {
        console.error('[Auth] 建立 SignalR 连接失败:', err)
        // 不影响初始化流程,继续
      }
    }

    return {
      token,
      refreshToken,
      tokenExpiresAt,
      loading,
      error,
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
  },
  {
    persist: {
      pick: ['token', 'refreshToken', 'tokenExpiresAt'],
    },
  },
)

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
