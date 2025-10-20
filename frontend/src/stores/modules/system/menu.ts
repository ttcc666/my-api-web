import { ref } from 'vue'
import { defineStore } from 'pinia'
import { AuthApi } from '@/api'
import type { MenuDto } from '@/types/api'
import { cacheConfig } from '@/config'
import { createAsyncCacheLoader } from '@/utils/asyncCacheLoader'

const MENU_TTL = cacheConfig.menuCacheExpiry

export const useMenuStore = defineStore(
  'menu',
  () => {
    const menus = ref<MenuDto[]>([])
    const loading = ref(false)
    const loaded = ref(false)
    const error = ref<string | null>(null)
    const lastLoadedAt = ref<number | null>(null)

    const loader = createAsyncCacheLoader<MenuDto[]>({
      ttl: MENU_TTL,
      lastLoadedAt,
      loading,
      isDataReady: () => loaded.value,
      fetcher: async () => {
        const data = await AuthApi.getCurrentUserMenus()
        return Array.isArray(data) ? data : []
      },
      onSuccess: (data) => {
        menus.value = data
        error.value = null
      },
      onError: (err) => {
        const message = err instanceof Error ? err.message : String(err)
        error.value = message || '加载菜单失败'
        lastLoadedAt.value = null
        menus.value = []
      },
      markLoaded: (value) => {
        loaded.value = value
      },
    })

    const loadMenus = async (forceRefresh = false) => {
      await loader.load(forceRefresh)
    }

    const ensureLoaded = async () => {
      if (!loader.isCacheValid()) {
        await loadMenus()
      }
    }

    const refreshMenus = async () => {
      await loader.refresh()
    }

    const clearMenus = () => {
      menus.value = []
      error.value = null
      loader.invalidate()
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
