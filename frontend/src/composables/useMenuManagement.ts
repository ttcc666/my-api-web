import { ref, readonly, computed } from 'vue'
import { MenusApi } from '@/api'
import type { MenuDto, CreateMenuDto, UpdateMenuDto } from '@/types/api'
import { useMenuStore } from '@/stores/modules/system/menu'

export function useMenuManagement() {
  const menuTree = ref<MenuDto[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  const fetchMenuTree = async () => {
    try {
      loading.value = true
      error.value = null

      const data = await MenusApi.getMenuTree()
      menuTree.value = Array.isArray(data) ? data : []
    } catch (err) {
      error.value = (err instanceof Error ? err.message : String(err)) || '加载菜单失败'
      throw err
    } finally {
      loading.value = false
    }
  }

  const syncMenuStore = async () => {
    const menuStore = useMenuStore()
    try {
      await menuStore.refreshMenus()
    } catch (err) {
      console.error('刷新导航菜单失败:', err)
    }
  }

  const createMenu = async (payload: CreateMenuDto) => {
    await MenusApi.createMenu(payload)
    await fetchMenuTree()
    await syncMenuStore()
  }

  const updateMenu = async (id: string, payload: UpdateMenuDto) => {
    await MenusApi.updateMenu(id, payload)
    await fetchMenuTree()
    await syncMenuStore()
  }

  const deleteMenu = async (id: string) => {
    await MenusApi.deleteMenu(id)
    await fetchMenuTree()
    await syncMenuStore()
  }

  return {
    menuTree: computed(() => menuTree.value),
    loading: readonly(loading),
    error: readonly(error),
    fetchMenuTree,
    createMenu,
    updateMenu,
    deleteMenu,
  }
}
