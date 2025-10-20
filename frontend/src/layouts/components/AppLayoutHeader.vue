<template>
  <a-layout-header class="layout-header" :style="headerStyle">
    <div class="logo">{{ title }}</div>

    <a-menu
      v-if="layoutMode === 'top'"
      mode="horizontal"
      :items="menuItems"
      :selectedKeys="selectedKeys"
      @click="handleMenuClick"
      class="top-menu"
    />

    <div class="user-info">
      <a-badge :count="onlineUserCount" :overflow-count="999" :offset="[-5, 5]">
        <a-button
          type="text"
          shape="circle"
          class="header-btn"
          title="在线用户"
          @click="emit('navigate-online')"
        >
          <template #icon>
            <TeamOutlined />
          </template>
        </a-button>
      </a-badge>
      <a-button
        type="text"
        shape="circle"
        class="header-btn"
        :loading="refreshing"
        @click="emit('refresh')"
      >
        <template #icon>
          <ReloadOutlined />
        </template>
      </a-button>
      <a-button type="text" shape="circle" class="header-btn" @click="emit('toggle-settings')">
        <template #icon>
          <SettingOutlined />
        </template>
      </a-button>
      <a-dropdown :trigger="['click']">
        <a-button type="text" class="user-button header-btn">
          <UserOutlined />
          <span class="username">{{ username }}</span>
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
</template>

<script setup lang="ts">
import type { MenuProps } from 'ant-design-vue'
import {
  ReloadOutlined,
  UserOutlined,
  LogoutOutlined,
  SettingOutlined,
  TeamOutlined,
} from '@ant-design/icons-vue'
import { computed } from 'vue'
import type { LayoutMode } from '@/stores/modules/common/theme'

const props = defineProps<{
  layoutMode: LayoutMode
  menuItems: MenuProps['items']
  selectedKeys: string[]
  onlineUserCount: number
  refreshing: boolean
  username: string
  primaryColor: string
  title?: string
}>()

const emit = defineEmits<{
  (e: 'menu-click', key: string): void
  (e: 'refresh'): void
  (e: 'toggle-settings'): void
  (e: 'user-menu-click', key: string): void
  (e: 'navigate-online'): void
}>()

const headerStyle = computed(() => ({
  '--primary-color': props.primaryColor,
}))

const title = computed(() => props.title ?? 'My App')

function handleMenuClick(info: Parameters<NonNullable<MenuProps['onClick']>>[0]) {
  emit('menu-click', String(info.key))
}

function handleUserMenuClick(info: Parameters<NonNullable<MenuProps['onClick']>>[0]) {
  emit('user-menu-click', String(info.key))
}

const username = computed(() => props.username || '用户')
</script>

<style scoped>
.layout-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 24px;
  background: #fff;
  border-bottom: 1px solid #f0f0f0;
}

.logo {
  font-size: 18px;
  font-weight: 600;
  color: #1f1f1f;
}

.top-menu {
  flex: 1;
  margin-left: 24px;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 12px;
}

.header-btn {
  color: rgba(0, 0, 0, 0.65);
  transition:
    color 0.3s,
    background 0.3s;
}

.header-btn:hover {
  background: color-mix(in srgb, var(--primary-color) 10%, transparent);
  color: var(--primary-color);
}

.header-btn :deep(.anticon) {
  color: inherit;
}

.user-button {
  display: flex;
  align-items: center;
  gap: 8px;
}

.user-button:hover .username {
  color: var(--primary-color);
}

.username {
  font-size: 14px;
  color: rgba(0, 0, 0, 0.65);
  transition: color 0.3s;
}
</style>
