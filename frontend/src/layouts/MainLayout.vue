<template>
  <n-layout style="height: 100vh">
    <n-layout-header bordered class="header">
      <div class="logo">My App</div>
      <div class="user-info">
        <span>欢迎，{{ authStore.username }}</span>
        <n-button text @click="handleLogout">退出登录</n-button>
      </div>
    </n-layout-header>
    <n-layout has-sider>
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
        />
      </n-layout-sider>
      <n-layout-content class="content">
        <router-view />
      </n-layout-content>
    </n-layout>
  </n-layout>
</template>

<script setup lang="ts">
import { h, ref, type Component } from 'vue'
import { NIcon } from 'naive-ui'
import { RouterLink, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import {
  HomeOutline as HomeIcon,
  PersonOutline as UserIcon
} from '@vicons/ionicons5'

const collapsed = ref(false)
const authStore = useAuthStore()
const router = useRouter()

function renderIcon (icon: Component) {
  return () => h(NIcon, null, { default: () => h(icon) })
}

const menuOptions = [
  {
    label: () => h(RouterLink, { to: '/' }, { default: () => '主页' }),
    key: 'home',
    icon: renderIcon(HomeIcon)
  },
  {
    label: () => h(RouterLink, { to: '/users' }, { default: () => '用户管理' }),
    key: 'users',
    icon: renderIcon(UserIcon)
  }
]

const handleLogout = () => {
  authStore.logout()
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
.content {
  padding: 24px;
  background-color: #f0f2f5;
}
</style>