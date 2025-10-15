<template>
  <a-layout class="layout-root">
    <a-layout-header class="layout-header">
      <div class="logo">My App</div>
      <div class="user-info">
        <a-button type="text" shape="circle" :loading="refreshing" @click="handleRefresh">
          <template #icon>
            <ReloadOutlined />
          </template>
        </a-button>
        <a-dropdown :trigger="['click']">
          <a-button type="text" class="user-button">
            <UserOutlined />
            <span class="username">{{ userStore.username || '用户' }}</span>
          </a-button>
          <template #overlay>
            <a-menu @click="handleUserMenuClick">
              <a-menu-item key="profile">
                <UserOutlined />
                <span>个人中心</span>
              </a-menu-item>
              <a-menu-item key="logout">
                <LogoutOutlined />
                <span>退出登录</span>
              </a-menu-item>
            </a-menu>
          </template>
        </a-dropdown>
      </div>
    </a-layout-header>
    <a-layout class="layout-body">
      <a-layout-sider collapsible :width="240" :collapsed="collapsed" @collapse="handleCollapse">
        <a-menu mode="inline" :items="menuItems" :selectedKeys="selectedMenuKeys"
          :openKeys="collapsed ? [] : openMenuKeys" :inline-collapsed="collapsed" @openChange="handleOpenChange"
          @click="handleMenuClick" />
      </a-layout-sider>
      <a-layout-content class="layout-content">
        <div class="content-navigation">
          <app-breadcrumb />
          <app-menu-tabs />
        </div>
        <div class="content-view">
          <router-view v-slot="{ Component }">
            <keep-alive :include="cachedViews" :max="10">
              <component :is="Component" />
            </keep-alive>
          </router-view>
        </div>
      </a-layout-content>
    </a-layout>
  </a-layout>
</template>

<script setup lang="ts">
import { h, ref, computed, watch, onMounted, type Component } from 'vue'
import type { DropdownProps, MenuProps } from 'ant-design-vue'
import { ReloadOutlined, UserOutlined, LogoutOutlined, HomeOutlined } from '@ant-design/icons-vue'
import { RouterLink, useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useUserStore } from '@/stores/user'
import { useMenuStore } from '@/stores/menu'
import { usePermissionStore } from '@/stores/permission'
import { useTabStore } from '@/stores/tabs'
import { message } from '@/plugins/antd'
import type { MenuDto } from '@/types/api'
import { getIconComponent } from '@/utils/iconRegistry'
import AppBreadcrumb from '@/components/navigation/AppBreadcrumb.vue'
import AppMenuTabs from '@/components/navigation/AppMenuTabs.vue'

type MenuItem = NonNullable<MenuProps['items']>[number]

const collapsed = ref(
  localStorage.getItem('menu-collapsed') ? localStorage.getItem('menu-collapsed') === 'true' : false
)
const refreshing = ref(false)
const openMenuKeys = ref<string[]>([])
const authStore = useAuthStore()
const userStore = useUserStore()
const menuStore = useMenuStore()
const permissionStore = usePermissionStore()
const router = useRouter()
const route = useRoute()
const tabStore = useTabStore()
const menuKeyParentMap = ref(new Map<string, string | null>())

const cachedViews = computed(() => tabStore.tabs.map((tab) => tab.key))

const selectedMenuKeys = computed(() => {
  const key = resolveActiveMenuKey()
  return key ? [key] : []
})

const fallbackMenuItems: MenuProps['items'] = [
  {
    key: 'home',
    icon: () => h(HomeOutlined),
    label: '主页',
  },
]

const menuItems = computed<MenuProps['items']>(() => {
  const menus = menuStore.menus
  const parentMap = new Map<string, string | null>()
  const items = buildMenuItems(Array.isArray(menus) ? menus : [], null, parentMap)
  menuKeyParentMap.value = parentMap
  return items.length > 0 ? items : fallbackMenuItems
})

watch(
  () => selectedMenuKeys.value[0],
  (activeKey) => {
    if (!activeKey) {
      openMenuKeys.value = []
      return
    }
    const map = menuKeyParentMap.value
    const ancestors: string[] = []
    let current = activeKey
    while (map.has(current)) {
      const parent = map.get(current)
      if (!parent) {
        break
      }
      ancestors.unshift(parent)
      current = parent
    }
    openMenuKeys.value = ancestors
  },
  { immediate: true },
)

watch(
  () => route.fullPath,
  () => {
    tabStore.syncWithRoute(route)
  },
  { immediate: true },
)

onMounted(async () => {
  // 确保菜单默认展开
  if (!localStorage.getItem('menu-collapsed')) {
    collapsed.value = false
    localStorage.setItem('menu-collapsed', 'false')
  }

  if (!authStore.isAuthenticated) {
    return
  }
  try {
    await menuStore.ensureLoaded()
  } catch (error) {
    console.error('加载菜单失败:', error)
  }
})

function handleUserMenuClick({ key }: { key: string }) {
  handleUserMenuSelect(key)
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

  if ((!children || children.length === 0) && !menu.routePath) {
    return null
  }

  map.set(key, parentKey)

  const iconComponent = resolveIcon(menu.icon ?? undefined)

  return {
    key,
    icon: iconComponent ? () => h(iconComponent) : undefined,
    label: createMenuLabel(menu),
    children: children.length > 0 ? children : undefined,
  }
}

function resolveMenuKey(menu: MenuDto): string {
  if (menu.routeName) {
    return menu.routeName
  }
  if (menu.routePath) {
    return menu.routePath
  }
  return menu.code || menu.id
}

function resolveActiveMenuKey(): string {
  if (typeof route.name === 'string') {
    return route.name
  }
  if (typeof route.meta?.rawPath === 'string') {
    return String(route.meta.rawPath)
  }
  return route.path
}

function createMenuLabel(menu: MenuDto) {
  return menu.title
}

function resolveIcon(name?: string | null): Component | null {
  return getIconComponent(name)
}

async function handleRefresh() {
  refreshing.value = true
  try {
    await Promise.all([permissionStore.loadUserPermissions(true), menuStore.refreshMenus()])
    message.success('刷新成功')
  } catch (error) {
    console.error('刷新失败:', error)
    message.error('刷新失败')
  } finally {
    refreshing.value = false
  }
}

function handleCollapse(value: boolean) {
  collapsed.value = value
  localStorage.setItem('menu-collapsed', String(value))
}

function handleOpenChange(keys: string[]) {
  openMenuKeys.value = keys
}

function handleMenuClick({ key }: { key: string }) {
  if (key === 'home') {
    router.push('/')
    return
  }
  const menu = findMenuByKey(menuStore.menus, key)
  if (menu?.routeName) {
    router.push({ name: menu.routeName })
  } else if (menu?.routePath) {
    router.push(menu.routePath)
  }
}

function findMenuByKey(menus: MenuDto[], key: string): MenuDto | null {
  for (const menu of menus) {
    const menuKey = resolveMenuKey(menu)
    if (menuKey === key) {
      return menu
    }
    if (menu.children) {
      const found = findMenuByKey(menu.children, key)
      if (found) {
        return found
      }
    }
  }
  return null
}

function handleUserMenuSelect(key: string) {
  if (key === 'profile') {
    router.push('/profile')
    return
  }
  if (key === 'logout') {
    handleLogout()
  }
}

function handleLogout() {
  authStore.logout()
  tabStore.reset()
  router.push('/login')
}
</script>

<style scoped>
.layout-root {
  min-height: 100vh;
}

.layout-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  height: 64px;
  padding: 0 24px;
  background: #fff;
  border-bottom: 1px solid #f0f0f0;
}

.logo {
  font-size: 20px;
  font-weight: 600;
  color: #1f1f1f;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 12px;
}

.user-button {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  color: #1f1f1f;
}

.username {
  font-size: 14px;
}

.layout-body {
  min-height: calc(100vh - 64px);
}

.layout-content {
  display: flex;
  flex-direction: column;
  background: #f5f5f5;
  height: calc(100vh - 64px);
  overflow: hidden;
}

.content-navigation {
  display: flex;
  flex-direction: column;
  gap: 3px;
  padding: 6px 24px 4px;
  background: #f5f5f5;
  border-bottom: 1px solid #eaeaea;
  flex-shrink: 0;
}

.content-view {
  flex: 1;
  padding: 24px;
  box-sizing: border-box;
  overflow-y: auto;
  overflow-x: hidden;
  scrollbar-width: none;
  /* Firefox */
  -ms-overflow-style: none;
  /* IE and Edge */
}

.content-view::-webkit-scrollbar {
  display: none;
  /* Chrome, Safari and Opera */
}

.content-view> :deep(*) {
  background: #fff;
  border-radius: 8px;
  padding: 16px;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.05);
}
</style>
