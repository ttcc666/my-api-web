import { h, ref, computed, watch, onMounted, onUnmounted, type Component } from 'vue'
import type { MenuProps } from 'ant-design-vue'
import { storeToRefs } from 'pinia'
import { HomeOutlined } from '@ant-design/icons-vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/modules/auth/auth'
import { useUserStore } from '@/stores/modules/system/user'
import { useMenuStore } from '@/stores/modules/system/menu'
import { usePermissionStore } from '@/stores/modules/system/permission'
import { useTabStore } from '@/stores/modules/common/tabs'
import { useOnlineUserStore } from '@/stores/modules/hub/onlineUser'
import { useFeedback } from '@/composables/useFeedback'
import type { MenuDto } from '@/types/api'
import { getIconComponent } from '@/utils/iconRegistry'
import { appConfig } from '@/config'

type MenuItem = NonNullable<MenuProps['items']>[number]

export function useMainLayout() {
  const collapsed = ref(localStorage.getItem('menu-collapsed') === 'true')
  const refreshing = ref(false)
  const openMenuKeys = ref<string[]>([])
  const showSettings = ref(false)
  const onlineUserCount = ref(0)
  let onlineUserCountTimer: number | null = null

  const authStore = useAuthStore()
  const userStore = useUserStore()
  const menuStore = useMenuStore()
  const { menus: menuList } = storeToRefs(menuStore)
  const permissionStore = usePermissionStore()
  const onlineUserStore = useOnlineUserStore()
  const tabStore = useTabStore()
  const router = useRouter()
  const route = useRoute()
  const { message: feedbackMessage } = useFeedback()

  const menuKeyParentMap = ref(new Map<string, string | null>())
  const cachedViews = computed(() => tabStore.tabs.map((tab) => tab.key))
  const displayUsername = computed(() => userStore.username || '用户')
  const appTitle = appConfig.title

  const selectedMenuKeys = computed(() => {
    const key = resolveActiveMenuKey()
    return key ? [key] : []
  })

  const fallbackMenuItems: MenuProps['items'] = [
    { key: 'home', icon: () => h(HomeOutlined), label: '主页' },
  ]

  const menuItems = computed<MenuProps['items']>(() => {
    const menus = menuList.value
    const parentMap = new Map<string, string | null>()
    const items = buildMenuItems(Array.isArray(menus) ? menus : [], null, parentMap)
    menuKeyParentMap.value = parentMap
    return items.length > 0 ? items : fallbackMenuItems
  })

  watch(
    [() => selectedMenuKeys.value[0], () => menuKeyParentMap.value],
    ([activeKey]) => {
      syncOpenMenuState(activeKey)
    },
    { immediate: true },
  )

  watch(
    () => route.fullPath,
    () => tabStore.syncWithRoute(route),
    { immediate: true },
  )

  onMounted(async () => {
    if (!authStore.isAuthenticated) return
    try {
      await menuStore.ensureLoaded()
    } catch (error) {
      console.error('加载菜单失败:', error)
    }

    await fetchOnlineUserCount()
    onlineUserCountTimer = window.setInterval(fetchOnlineUserCount, 30_000)
  })

  onUnmounted(() => {
    if (onlineUserCountTimer) {
      clearInterval(onlineUserCountTimer)
      onlineUserCountTimer = null
    }
  })

  async function fetchOnlineUserCount() {
    if (!authStore.isAuthenticated) return
    try {
      onlineUserCount.value = await onlineUserStore.fetchCount()
    } catch (error) {
      console.error('获取在线用户数量失败:', error)
    }
  }

  function buildMenuItems(
    menus: MenuDto[],
    parentKey: string | null,
    map: Map<string, string | null>,
  ): MenuItem[] {
    return menus
      .map((menu) => transformMenuToItem(menu, parentKey, map))
      .filter((item): item is MenuItem => item !== null)
  }

  function transformMenuToItem(
    menu: MenuDto,
    parentKey: string | null,
    map: Map<string, string | null>,
  ): MenuItem | null {
    const key = resolveMenuKey(menu)
    const children = buildMenuItems(menu.children ?? [], key, map)
    if (children.length === 0 && !menu.routePath) return null
    map.set(key, parentKey)
    const iconComponent = resolveIcon(menu.icon ?? undefined)
    return {
      key,
      icon: iconComponent ? () => h(iconComponent) : undefined,
      label: menu.title,
      children: children.length > 0 ? children : undefined,
    }
  }

  function resolveMenuKey(menu: MenuDto): string {
    return menu.routeName || menu.routePath || menu.code || menu.id
  }

  function resolveActiveMenuKey(): string {
    return String(route.name || route.meta.rawPath || route.path)
  }

  function resolveIcon(name?: string | null): Component | null {
    return getIconComponent(name)
  }

  async function handleRefresh() {
    refreshing.value = true
    try {
      await Promise.all([permissionStore.loadUserPermissions(true), menuStore.refreshMenus()])
      feedbackMessage.success('刷新成功')
    } catch (error) {
      console.error('刷新失败:', error)
      feedbackMessage.error('刷新失败')
    } finally {
      refreshing.value = false
    }
  }

  function handleCollapse(value: boolean) {
    collapsed.value = value
    localStorage.setItem('menu-collapsed', String(value))
    if (!value) {
      syncOpenMenuState(selectedMenuKeys.value[0])
    }
  }

  function handleOpenChange(keys: string[]) {
    openMenuKeys.value = keys
  }

  function handleMenuClick(key: string) {
    if (key === 'home') {
      router.push('/')
      return
    }
    const menu = findMenuByKey(menuList.value, key)
    if (menu?.routeName) router.push({ name: menu.routeName })
    else if (menu?.routePath) router.push(menu.routePath)
  }

  function collectAncestorKeys(key: string): string[] {
    const map = menuKeyParentMap.value
    const ancestors: string[] = []
    let current = key
    while (map.has(current)) {
      const parent = map.get(current)
      if (!parent) break
      ancestors.unshift(parent)
      current = parent
    }
    return ancestors
  }

  function syncOpenMenuState(explicitKey?: string | null) {
    if (collapsed.value) return
    const activeKey = explicitKey ?? selectedMenuKeys.value[0]
    if (!activeKey) {
      openMenuKeys.value = []
      return
    }
    openMenuKeys.value = collectAncestorKeys(activeKey)
  }

  function findMenuByKey(menus: MenuDto[], key: string): MenuDto | null {
    for (const menu of menus) {
      if (resolveMenuKey(menu) === key) return menu
      if (menu.children) {
        const found = findMenuByKey(menu.children, key)
        if (found) return found
      }
    }
    return null
  }

  function handleUserMenuClick(key: string) {
    if (key === 'profile') router.push('/profile')
    else if (key === 'logout') handleLogout()
  }

  function handleLogout() {
    authStore.logout()
    tabStore.reset()
    router.push('/login')
  }

  function navigateToOnlineUsers() {
    router.push({ name: 'OnlineUserManagement' })
  }

  return {
    collapsed,
    refreshing,
    openMenuKeys,
    showSettings,
    onlineUserCount,
    menuItems,
    selectedMenuKeys,
    cachedViews,
    displayUsername,
    appTitle,
    handleRefresh,
    handleCollapse,
    handleOpenChange,
    handleMenuClick,
    handleUserMenuClick,
    navigateToOnlineUsers,
  }
}
