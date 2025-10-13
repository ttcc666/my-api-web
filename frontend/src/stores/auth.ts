import { ref, computed } from 'vue'
import { defineStore } from 'pinia'
import type { User, UserLoginDto, UserRegisterDto } from '@/types/api'
import UserApi from '@/api/user'

export const useAuthStore = defineStore('auth', () => {
  // 状态
  const user = ref<User | null>(null)
  const token = ref<string | null>(localStorage.getItem('accessToken'))
  const refreshToken = ref<string | null>(localStorage.getItem('refreshToken'))
  const loading = ref(false)
  const error = ref<string | null>(null)

  // 计算属性
  const isAuthenticated = computed(() => !!token.value)
  const username = computed(() => user.value?.username || '')

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

  // 登出
  const logout = () => {
    user.value = null
    token.value = null
    refreshToken.value = null
    localStorage.removeItem('accessToken')
    localStorage.removeItem('refreshToken')
    localStorage.removeItem('user')
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
    }
  }

  return {
    // 状态
    user,
    token, // 实际为 accessToken
    refreshToken,
    loading,
    error,
    // 计算属性
    isAuthenticated,
    username,
    // 方法
    clearError,
    setUser,
    setTokens,
    login,
    register,
    logout,
    fetchUserInfo,
    initializeAuth
  }
})