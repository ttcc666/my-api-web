<template>
  <a-layout class="layout-root">
    <!-- 统一的顶部导航栏 -->
    <a-layout-header class="layout-header">
      <div class="logo">My App</div>

      <!-- 顶部菜单，仅在 top 模式下显示 -->
      <a-menu v-if="themeStore.layoutMode === 'top'" mode="horizontal" :items="menuItems"
        :selectedKeys="selectedMenuKeys" @click="handleMenuClick" class="top-menu" />

      <!-- 右侧用户信息和操作按钮 -->
      <div class="user-info">
        <a-button type="text" shape="circle" :loading="refreshing" @click="handleRefresh" class="header-btn">
          <template #icon>
            <ReloadOutlined />
          </template>
        </a-button>
        <a-button type="text" shape="circle" @click="showSettings = true" class="header-btn">
          <template #icon>
            <SettingOutlined />
          </template>
        </a-button>
        <a-dropdown :trigger="['click']">
          <a-button type="text" class="user-button header-btn">
            <UserOutlined />
            <span class="username">{{ userStore.username || '用户' }}</span>
          </a-button>
          <template #overlay>
            <a-menu @click="handleUserMenuClick">
              <a-menu-item key="profile">
                <UserOutlined /><span>个人中心</span>
              </a-menu-item>
              <a-menu-item key="logout">
                <LogoutOutlined /><span>退出登录</span>
              </a-menu-item>
            </a-menu>
          </template>
        </a-dropdown>
      </div>
    </a-layout-header>

    <!-- 主体内容区域 -->
    <a-layout class="layout-body">
      <!-- 侧边栏，非 top 模式下显示 -->
      <a-layout-sider v-if="themeStore.layoutMode !== 'top'" collapsible :width="240" :collapsed="collapsed"
        :collapsed-width="60" @collapse="handleCollapse" class="custom-sider">
        <a-menu mode="inline" :items="menuItems" :selectedKeys="selectedMenuKeys"
          :openKeys="collapsed ? [] : openMenuKeys" @openChange="handleOpenChange" @click="handleMenuClick"
          class="custom-menu" />
      </a-layout-sider>

      <!-- 内容区，使用独立 a-layout 包裹以隔离背景 -->
      <a-layout class="content-wrapper">
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

    <!-- 主题设置抽屉 -->
    <theme-settings :visible="showSettings" @update:visible="showSettings = $event" />
  </a-layout>
</template>

<script setup lang="ts">
import { h, ref, computed, watch, onMounted, type Component } from 'vue'
import type { MenuProps } from 'ant-design-vue'
import { ReloadOutlined, UserOutlined, LogoutOutlined, HomeOutlined, SettingOutlined } from '@ant-design/icons-vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useUserStore } from '@/stores/user'
import { useMenuStore } from '@/stores/menu'
import { usePermissionStore } from '@/stores/permission'
import { useTabStore } from '@/stores/tabs'
import { useThemeStore } from '@/stores/theme'
import { message } from '@/plugins/antd'
import type { MenuDto } from '@/types/api'
import { getIconComponent } from '@/utils/iconRegistry'
import AppBreadcrumb from '@/components/navigation/AppBreadcrumb.vue'
import AppMenuTabs from '@/components/navigation/AppMenuTabs.vue'
import ThemeSettings from '@/components/ThemeSettings.vue'

type MenuItem = NonNullable<MenuProps['items']>[number]

const collapsed = ref(localStorage.getItem('menu-collapsed') === 'true')
const refreshing = ref(false)
const openMenuKeys = ref<string[]>([])
const showSettings = ref(false)

const authStore = useAuthStore()
const userStore = useUserStore()
const menuStore = useMenuStore()
const permissionStore = usePermissionStore()
const themeStore = useThemeStore()
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
  { key: 'home', icon: () => h(HomeOutlined), label: '主页' },
]

const menuItems = computed<MenuProps['items']>(() => {
  const menus = menuStore.menus
  const parentMap = new Map<string, string | null>()
  const items = buildMenuItems(Array.isArray(menus) ? menus : [], null, parentMap)
  menuKeyParentMap.value = parentMap
  return items.length > 0 ? items : fallbackMenuItems
})

watch(() => selectedMenuKeys.value[0], (activeKey) => {
  if (!activeKey || collapsed.value) {
    return
  }
  const map = menuKeyParentMap.value
  const ancestors: string[] = []
  let current = activeKey
  while (map.has(current)) {
    const parent = map.get(current)
    if (!parent) break
    ancestors.unshift(parent)
    current = parent
  }
  openMenuKeys.value = ancestors
}, { immediate: true })

watch(() => route.fullPath, () => tabStore.syncWithRoute(route), { immediate: true })

onMounted(async () => {
  if (!authStore.isAuthenticated) return
  try {
    await menuStore.ensureLoaded()
  } catch (error) {
    console.error('加载菜单失败:', error)
  }
})

function buildMenuItems(menus: MenuDto[], parentKey: string | null, map: Map<string, string | null>): MenuItem[] {
  return menus.map((menu) => transformMenuToItem(menu, parentKey, map)).filter((item): item is MenuItem => item !== null)
}

function transformMenuToItem(menu: MenuDto, parentKey: string | null, map: Map<string, string | null>): MenuItem | null {
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
    message.success('刷新成功')
  } catch (error) {
    message.error('刷新失败')
  } finally {
    refreshing.value = false
  }
}

function handleCollapse(value: boolean) {
  collapsed.value = value
  localStorage.setItem('menu-collapsed', String(value))
  if (!value) {
    // 重新计算展开的菜单
    const activeKey = selectedMenuKeys.value[0]
    if (!activeKey) return
    const map = menuKeyParentMap.value
    const ancestors: string[] = []
    let current = activeKey
    while (map.has(current)) {
      const parent = map.get(current)
      if (!parent) break
      ancestors.unshift(parent)
      current = parent
    }
    openMenuKeys.value = ancestors
  }
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
  if (menu?.routeName) router.push({ name: menu.routeName })
  else if (menu?.routePath) router.push(menu.routePath)
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

function handleUserMenuClick({ key }: { key: string }) {
  if (key === 'profile') router.push('/profile')
  else if (key === 'logout') handleLogout()
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
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.08);
  transition: all 0.3s;
  position: relative;
  z-index: 10;
}


.logo {
  font-size: 22px;
  font-weight: 700;
  flex-shrink: 0;
  color: v-bind('themeStore.primaryColor');
  transition: color 0.3s;
  letter-spacing: 0.5px;
}

.top-menu {
  flex: 1;
  border: none !important;
  background: transparent !important;
  margin: 0 24px;
  line-height: 61px;
  /* 64px - 3px for border */
}

.top-menu :deep(.ant-menu-item) {
  color: rgba(0, 0, 0, 0.65);
  border-bottom-width: 3px !important;
}

.top-menu :deep(.ant-menu-item-selected) {
  color: v-bind('themeStore.primaryColor') !important;
  border-bottom-color: v-bind('themeStore.primaryColor') !important;
}

.top-menu :deep(.ant-menu-item:hover) {
  color: v-bind('themeStore.primaryColor') !important;
  border-bottom-color: v-bind('themeStore.primaryColor') !important;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-shrink: 0;
}

.header-btn,
.user-button {
  transition: all 0.3s;
  color: rgba(0, 0, 0, 0.65);
}

.header-btn :deep(.anticon),
.user-button :deep(.anticon) {
  color: rgba(0, 0, 0, 0.65);
  transition: color 0.3s;
}

.header-btn:hover,
.user-button:hover {
  background: v-bind('`${themeStore.primaryColor}1A`') !important;
  color: v-bind('themeStore.primaryColor') !important;
}

.header-btn:hover :deep(.anticon),
.user-button:hover :deep(.anticon) {
  color: v-bind('themeStore.primaryColor') !important;
}

.user-button:hover .username {
  color: v-bind('themeStore.primaryColor') !important;
}

.username {
  font-size: 14px;
  transition: color 0.3s;
}

.layout-body {
  min-height: calc(100vh - 64px);
}

.custom-sider {
  background: v-bind('themeStore.primaryColor') !important;
  box-shadow: 2px 0 8px rgba(0, 0, 0, 0.15);
  z-index: 9;
}

.custom-sider :deep(.ant-layout-sider-trigger) {
  background: v-bind('themeStore.primaryColor') !important;
  filter: brightness(0.95);
}

.custom-menu {
  background: transparent !important;
  border-right: 0 !important;
}

.custom-menu :deep(.ant-menu-item),
.custom-menu :deep(.ant-menu-submenu-title) {
  color: rgba(255, 255, 255, 0.85) !important;
  margin: 4px 8px !important;
  width: calc(100% - 16px) !important;
  border-radius: 6px !important;
}

.ant-layout-sider-collapsed .custom-menu :deep(.ant-menu-item),
.ant-layout-sider-collapsed .custom-menu :deep(.ant-menu-submenu-title) {
  padding: 0 !important;
  margin: 4px auto !important;
  width: 44px !important;
  height: 44px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.ant-layout-sider-collapsed .custom-menu :deep(.ant-menu-item-icon),
.ant-layout-sider-collapsed .custom-menu :deep(.ant-menu-submenu-arrow) {
  margin: 0 !important;
}

.ant-layout-sider-collapsed .custom-menu :deep(.ant-menu-title-content) {
  display: none;
}

.custom-menu :deep(.ant-menu-item:hover),
.custom-menu :deep(.ant-menu-submenu-title:hover) {
  background: rgba(255, 255, 255, 0.15) !important;
  color: #fff !important;
}

.custom-menu :deep(.ant-menu-item-selected) {
  background: rgba(255, 255, 255, 0.25) !important;
  color: #fff !important;
}

.custom-menu :deep(.ant-menu-submenu-selected > .ant-menu-submenu-title) {
  color: #fff !important;
}

.custom-menu :deep(.ant-menu-item-icon),
.custom-menu :deep(.ant-menu-submenu-arrow) {
  color: rgba(255, 255, 255, 0.85) !important;
  transition: color 0.3s;
}

.custom-menu :deep(.ant-menu-item:hover .ant-menu-item-icon),
.custom-menu :deep(.ant-menu-item-selected .ant-menu-item-icon) {
  color: #fff !important;
}

.custom-menu :deep(.ant-menu-sub) {
  background: rgba(0, 0, 0, 0.15) !important;
}

.content-wrapper {
  background: #f0f2f5;
}

.layout-content {
  display: flex;
  flex-direction: column;
  height: calc(100vh - 64px);
  overflow: hidden;
}

.content-navigation {
  padding: 8px 24px;
  background: #fff;
  border-bottom: 1px solid #f0f0f0;
  flex-shrink: 0;
}

.content-view {
  flex: 1;
  padding: 24px;
  box-sizing: border-box;
  overflow-y: auto;
}
</style>
