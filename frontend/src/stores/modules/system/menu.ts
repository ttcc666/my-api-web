import { ref } from 'vue'
import { defineStore } from 'pinia'
import { AuthApi } from '@/api'
import type { MenuDto } from '@/types/api'
import { cacheConfig } from '@/config'

const MENU_TTL = cacheConfig.menuCacheExpiry

export const useMenuStore = defineStore(
  'menu',
  () => {
    const menus = ref<MenuDto[]>([])
    const loading = ref(false)
    const loaded = ref(false)
    const error = ref<string | null>(null)
    const lastLoadedAt = ref<number | null>(null)

    let loadPromise: Promise<void> | null = null

    const isCacheValid = () => {
      if (!loaded.value || !lastLoadedAt.value) {
        return false
      }
      return Date.now() - lastLoadedAt.value < MENU_TTL
    }

    const fetchMenus = async (forceRefresh = false) => {
      if (!forceRefresh && isCacheValid()) {
        return
      }

      loading.value = true
      error.value = null

      try {
        const data = await AuthApi.getCurrentUserMenus()
        menus.value = Array.isArray(data) ? data : []
        loaded.value = true
        lastLoadedAt.value = Date.now()
      } catch (err) {
        error.value = (err instanceof Error ? err.message : String(err)) || '加载菜单失败'
        loaded.value = false
        lastLoadedAt.value = null
        throw err
      } finally {
        loading.value = false
      }
    }

    const loadMenus = async () => {
      if (isCacheValid()) {
        return
      }

      if (loadPromise) {
        return loadPromise
      }

      loadPromise = (async () => {
        try {
          await fetchMenus()
        } finally {
          loadPromise = null
        }
      })()

      return loadPromise
    }

    const ensureLoaded = async () => {
      if (!isCacheValid()) {
        await loadMenus()
      }
    }

    const refreshMenus = async () => {
      loaded.value = false
      lastLoadedAt.value = null
      loadPromise = null
      await fetchMenus(true)
    }

    const clearMenus = () => {
      menus.value = []
      loading.value = false
      loaded.value = false
      error.value = null
      lastLoadedAt.value = null
      loadPromise = null
    }

    return {
      menus,
      loading,
      loaded,
      error,
      lastLoadedAt,
      loadMenus,
      ensureLoaded,
      refreshMenus,
      clearMenus,
    }
  },
  {
    persist: {
      pick: ['menus', 'loaded', 'lastLoadedAt'],
    },
  },
)
