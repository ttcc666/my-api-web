<template>
  <n-layout style="height: 100vh">
    <n-layout-header bordered class="header">
      <div class="logo">My App</div>
      <div class="user-info">
        <span>欢迎，{{ authStore.username }}</span>
        <n-button text @click="handleLogout">退出登录</n-button>
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
          <keep-alive>
            <router-view />
          </keep-alive>
        </div>
      </n-layout-content>
    </n-layout>
  </n-layout>
</template>

<script setup lang="ts">
import { h, ref, computed, watch, type Component } from 'vue'
import { NIcon } from 'naive-ui'
import { RouterLink, useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useTabStore } from '@/stores/tabs'
import {
  HomeOutline as HomeIcon,
  PersonOutline as UserIcon,
  ShieldCheckmarkOutline as ShieldIcon,
  SettingsOutline as SettingsIcon,
} from '@vicons/ionicons5'
import AppBreadcrumb from '@/components/navigation/AppBreadcrumb.vue'
import AppMenuTabs from '@/components/navigation/AppMenuTabs.vue'

const collapsed = ref(false)
const authStore = useAuthStore()
const router = useRouter()
const route = useRoute()
const tabStore = useTabStore()

// 计算当前菜单项的 key，用于高亮显示
const currentMenuKey = computed(() => {
  return route.name as string
})

function renderIcon(icon: Component) {
  return () => h(NIcon, null, { default: () => h(icon) })
}

const menuOptions = [
  {
    label: () => h(RouterLink, { to: '/' }, { default: () => '主页' }),
    key: 'home',
    icon: renderIcon(HomeIcon),
  },
  {
    label: '系统管理',
    key: 'system-management',
    icon: renderIcon(SettingsIcon),
    show: authStore.hasPermission('user:view') || authStore.hasPermission('role:view'),
    children: [
      {
        label: () => h(RouterLink, { to: '/admin/users' }, { default: () => '用户管理' }),
        key: 'user-management',
        icon: renderIcon(UserIcon),
        show: authStore.hasPermission('user:view'),
      },
      {
        label: () => h(RouterLink, { to: '/admin/roles' }, { default: () => '角色管理' }),
        key: 'role-management',
        icon: renderIcon(ShieldIcon),
        show: authStore.hasPermission('role:view'),
      },
    ],
  },
]

watch(
  () => route.fullPath,
  () => {
    tabStore.syncWithRoute(route)
  },
  { immediate: true },
)

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
  position: relative;
  padding: 0;
  height: 100%;
  background-color: #f0f2f5;
  overflow: hidden;
}
.content-navigation {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  z-index: 100;
  display: flex;
  flex-direction: column;
  gap: 12px;
  padding: 24px 24px 16px 24px;
  background-color: #f0f2f5;
  border-bottom: 1px solid #e0e0e6;
}
.content-view {
  height: 100%;
  padding-top: 120px;
  padding-left: 24px;
  padding-right: 24px;
  padding-bottom: 24px;
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
  min-height: calc(100vh - 200px);
}
</style>
