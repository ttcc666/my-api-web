import { ref, readonly, computed } from 'vue'
import { defineStore } from 'pinia'
import { AuthApi } from '@/api'
import type { MenuDto } from '@/types/api'
import { menuCache } from '@/utils/cache'

export const useMenuStore = defineStore('menu', () => {
  const menus = ref<MenuDto[]>([])
  const loading = ref(false)
  const loaded = ref(false)
  const error = ref<string | null>(null)

  let loadPromise: Promise<void> | null = null

  const fetchMenus = async (forceRefresh = false) => {
    if (forceRefresh) {
      menuCache.remove()
    }

    const cached = menuCache.get() as MenuDto[] | null
    if (cached && !forceRefresh) {
      menus.value = cached
      loaded.value = true
      return
    }

    loading.value = true
    error.value = null

    try {
      const data = await AuthApi.getCurrentUserMenus()
      menus.value = Array.isArray(data) ? data : []
      menuCache.set(menus.value)
      loaded.value = true
    } catch (err) {
      error.value = (err instanceof Error ? err.message : String(err)) || '加载菜单失败'
      loaded.value = false
      throw err
    } finally {
      loading.value = false
    }
  }

  const loadMenus = async () => {
    if (loaded.value) {
      return
    }

    if (loadPromise) {
      return loadPromise
    }

    loadPromise = (async () => {
      try {
        await fetchMenus(false)
      } finally {
        loadPromise = null
      }
    })()

    return loadPromise
  }

  const ensureLoaded = async () => {
    if (!loaded.value) {
      await loadMenus()
    }
  }

  const refreshMenus = async () => {
    loaded.value = false
    loadPromise = null
    await fetchMenus(true)
  }

  const clearMenus = () => {
    menus.value = []
    loading.value = false
    loaded.value = false
    error.value = null
    loadPromise = null
  }

  return {
    menus: computed(() => menus.value),
    loading: readonly(loading),
    loaded: readonly(loaded),
    error: readonly(error),
    loadMenus,
    ensureLoaded,
    refreshMenus,
    clearMenus,
  }
})
