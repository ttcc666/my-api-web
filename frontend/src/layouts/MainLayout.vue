<template>
  <a-layout class="layout-root">
    <a-layout-header class="layout-header">
      <div class="logo">My App</div>
      <div class="user-info">
        <a-button
          type="text"
          shape="circle"
          :loading="refreshing"
          @click="handleRefresh"
        >
          <ReloadOutlined />
        </a-button>
        <a-dropdown :trigger="['click']" :menu="userDropdownMenu">
          <a-button type="text" class="user-button">
            <UserOutlined />
            <span class="username">{{ userStore.username || '用户' }}</span>
          </a-button>
        </a-dropdown>
      </div>
    </a-layout-header>
    <a-layout class="layout-body">
      <a-layout-sider
        collapsible
        :width="240"
        :collapsed="collapsed"
        @collapse="handleCollapse"
      >
        <a-menu
          mode="inline"
          :items="menuItems"
          :selectedKeys="selectedMenuKeys"
          :openKeys="collapsed ? [] : openMenuKeys"
          :inline-collapsed="collapsed"
          @openChange="handleOpenChange"
        />
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

const collapsed = ref(false)
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
    label: () => h(RouterLink, { to: '/' }, { default: () => '主页' }),
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
  if (!authStore.isAuthenticated) {
    return
  }
  try {
    await menuStore.ensureLoaded()
  } catch (error) {
    console.error('加载菜单失败:', error)
  }
})

const userDropdownMenu = computed<DropdownProps['menu']>(() => ({
  items: [
    {
      key: 'profile',
      label: '个人中心',
      icon: () => h(UserOutlined),
    },
    {
      key: 'logout',
      label: '退出登录',
      icon: () => h(LogoutOutlined),
    },
  ],
  onClick: ({ key }: { key: string | number }) => {
    handleUserMenuSelect(String(key))
  },
}))

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
  const target = resolveMenuTarget(menu)
  if (target) {
    return () => h(RouterLink, { to: target }, { default: () => menu.title })
  }
  return menu.title
}

function resolveMenuTarget(menu: MenuDto) {
  if (menu.routeName) {
    return { name: menu.routeName }
  }
  if (menu.routePath) {
    return menu.routePath
  }
  return undefined
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
}

function handleOpenChange(keys: string[]) {
  openMenuKeys.value = keys
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
  min-height: calc(100vh - 64px);
}

.content-navigation {
  display: flex;
  flex-direction: column;
  gap: 12px;
  padding: 24px 24px 16px;
  background: #f5f5f5;
  border-bottom: 1px solid #eaeaea;
  position: sticky;
  top: 0;
  z-index: 10;
}

.content-view {
  flex: 1;
  padding: 24px;
  box-sizing: border-box;
  overflow: auto;
}

.content-view > div {
  background: #fff;
  border-radius: 8px;
  padding: 16px;
  min-height: 100%;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.05);
}
</style>
