<template>
  <n-layout style="height: 100vh">
    <n-layout-header bordered class="header">
      <div class="logo">My App</div>
      <div class="user-info">
        <n-button quaternary circle :loading="refreshing" @click="handleRefresh">
          <template #icon>
            <n-icon><refresh-outline /></n-icon>
          </template>
        </n-button>
        <n-dropdown :options="userMenuOptions" @select="handleUserMenuSelect">
          <n-button text>
            <template #icon>
              <n-icon><person-outline /></n-icon>
            </template>
            {{ userStore.username || '用户' }}
          </n-button>
        </n-dropdown>
      </div>
    </n-layout-header>
    <n-layout has-sider class="main-container">
      <n-layout-sider
        bordered
        collapse-mode="width"
        :collapsed-width="64"
        :width="240"
        :collapsed="collapsed"
        show-trigger
        @collapse="collapsed = true"
        @expand="collapsed = false"
      >
        <n-menu
          :collapsed="collapsed"
          :collapsed-width="64"
          :collapsed-icon-size="22"
          :options="menuOptions"
          :value="currentMenuKey"
        />
      </n-layout-sider>
      <n-layout-content class="content">
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
      </n-layout-content>
    </n-layout>
  </n-layout>
</template>

<script setup lang="ts">
import { h, ref, computed, watch, onMounted, type Component } from 'vue'
import { NIcon, type MenuOption, type DropdownOption, useMessage } from 'naive-ui'
import { RefreshOutline, PersonOutline, LogOutOutline } from '@vicons/ionicons5'
import { RouterLink, useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useUserStore } from '@/stores/user'
import { useMenuStore } from '@/stores/menu'
import { usePermissionStore } from '@/stores/permission'
import { useTabStore } from '@/stores/tabs'
import type { MenuDto } from '@/types/api'
import { getIconComponent } from '@/utils/iconRegistry'
import AppBreadcrumb from '@/components/navigation/AppBreadcrumb.vue'
import AppMenuTabs from '@/components/navigation/AppMenuTabs.vue'

const message = useMessage()

const collapsed = ref(false)
const refreshing = ref(false)
const authStore = useAuthStore()
const userStore = useUserStore()
const menuStore = useMenuStore()
const permissionStore = usePermissionStore()
const router = useRouter()
const route = useRoute()
const tabStore = useTabStore()

const homeMenuIcon = resolveIcon('HomeOutline')

const defaultMenuOptions: MenuOption[] = [
  {
    label: () => h(RouterLink, { to: '/' }, { default: () => '主页' }),
    key: 'home',
    icon: homeMenuIcon ? renderIcon(homeMenuIcon) : undefined,
  },
]

const userMenuOptions: DropdownOption[] = [
  {
    label: '个人中心',
    key: 'profile',
    icon: renderIcon(PersonOutline),
  },
  {
    label: '退出登录',
    key: 'logout',
    icon: renderIcon(LogOutOutline),
  },
]

// Keep-alive 缓存的视图列表（基于打开的标签页）
const cachedViews = computed(() => {
  return tabStore.tabs.map((tab) => tab.key)
})

// 计算当前菜单项的 key，用于高亮显示
const currentMenuKey = computed(() => {
  return route.name as string
})

function renderIcon(icon: Component) {
  return () => h(NIcon, null, { default: () => h(icon) })
}

const menuOptions = computed<MenuOption[]>(() => {
  const menus = menuStore.menus
  if (!menus || menus.length === 0) {
    return defaultMenuOptions
  }

  const options = menus
    .map(transformMenuToOption)
    .filter((option): option is MenuOption => option !== null)

  return options.length > 0 ? options : defaultMenuOptions
})

function resolveIcon(name?: string | null): Component | null {
  return getIconComponent(name)
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

function createMenuLabel(menu: MenuDto) {
  if (menu.routePath) {
    const target = menu.routePath
    return () => h(RouterLink, { to: target }, { default: () => menu.title })
  }
  return menu.title
}

function transformMenuToOption(menu: MenuDto): MenuOption | null {
  const children = menu.children
    ?.map(transformMenuToOption)
    .filter((child): child is MenuOption => child !== null)

  if ((!children || children.length === 0) && !menu.routePath) {
    return null
  }

  const iconComponent = resolveIcon(menu.icon ?? undefined)

  const option: MenuOption = {
    label: createMenuLabel(menu),
    key: resolveMenuKey(menu),
    icon: iconComponent ? renderIcon(iconComponent) : undefined,
  }

  if (children && children.length > 0) {
    option.children = children
  }

  return option
}

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

const handleRefresh = async () => {
  refreshing.value = true
  try {
    await Promise.all([permissionStore.loadUserPermissions(true), menuStore.refreshMenus()])
    message.success('刷新成功')
  } catch (error) {
    message.error('刷新失败')
    console.error('刷新失败:', error)
  } finally {
    refreshing.value = false
  }
}

const handleUserMenuSelect = (key: string) => {
  if (key === 'profile') {
    router.push('/profile')
  } else if (key === 'logout') {
    handleLogout()
  }
}

const handleLogout = () => {
  authStore.logout()
  tabStore.reset()
  router.push('/login')
}
</script>

<style scoped>
.header {
  height: 64px;
  padding: 0 24px;
  display: flex;
  align-items: center;
  justify-content: space-between;
}
.logo {
  font-size: 20px;
  font-weight: bold;
}
.user-info {
  display: flex;
  align-items: center;
  gap: 16px;
}
.main-container {
  height: calc(100vh - 64px);
}
.content {
  padding: 0;
  height: 100%;
  background-color: #f0f2f5;
  display: flex;
  flex-direction: column;
}
.content-navigation {
  position: sticky;
  top: 0;
  z-index: 100;
  display: flex;
  flex-direction: column;
  gap: 12px;
  padding: 24px 24px 16px 24px;
  background-color: #f0f2f5;
  border-bottom: 1px solid #e0e0e6;
  flex-shrink: 0;
}
.content-view {
  flex: 1;
  padding: 24px;
  box-sizing: border-box;
  overflow-y: auto;
  /* 隐藏滚动条但保持滚动功能 */
  scrollbar-width: none; /* Firefox */
  -ms-overflow-style: none; /* IE and Edge */
}
.content-view::-webkit-scrollbar {
  display: none; /* Chrome, Safari, Opera */
}
.content-view > div {
  background-color: #fff;
  border-radius: 8px;
  padding: 16px;
  min-height: 100%;
}
</style>
