import { ref, computed, readonly } from 'vue'
import { defineStore } from 'pinia'
import type { UserDto } from '@/types/api'
import { UsersApi } from '@/api'

type User = UserDto

export const useUserStore = defineStore('user', () => {
  const user = ref<User | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  const username = computed(() => user.value?.username || '')

  const setUser = (userData: User) => {
    user.value = userData
    localStorage.setItem('user', JSON.stringify(userData))
  }

  const clearUser = () => {
    user.value = null
    localStorage.removeItem('user')
  }

  const fetchUserInfo = async (): Promise<boolean> => {
    try {
      loading.value = true
      const userData = await UsersApi.getProfile()
      setUser(userData)
      return true
    } catch (err: unknown) {
      error.value = (err as Error).message || '获取用户信息失败'
      return false
    } finally {
      loading.value = false
    }
  }

  const initializeUser = () => {
    const storedUser = localStorage.getItem('user')
    if (storedUser) {
      try {
        user.value = JSON.parse(storedUser)
      } catch {
        clearUser()
      }
    }
  }

  return {
    user: readonly(user),
    loading: readonly(loading),
    error: readonly(error),
    username,
    setUser,
    clearUser,
    fetchUserInfo,
    initializeUser,
  }
})
