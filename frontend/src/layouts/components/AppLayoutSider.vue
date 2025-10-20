<template>
  <a-layout-sider
    v-if="layoutMode !== 'top'"
    collapsible
    :width="240"
    :collapsed="collapsed"
    :collapsed-width="60"
    class="layout-sider"
    :style="siderStyle"
    @collapse="emit('update:collapsed', $event)"
  >
    <a-menu
      mode="inline"
      :items="menuItems"
      :selectedKeys="selectedKeys"
      :openKeys="collapsed ? [] : openKeys"
      @openChange="handleOpenChange"
      @click="handleMenuClick"
      class="layout-menu"
    />
  </a-layout-sider>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { MenuProps } from 'ant-design-vue'
import type { LayoutMode } from '@/stores/modules/common/theme'

const props = defineProps<{
  layoutMode: LayoutMode
  collapsed: boolean
  menuItems: MenuProps['items']
  selectedKeys: string[]
  openKeys: string[]
  primaryColor: string
}>()

const emit = defineEmits<{
  (e: 'update:collapsed', value: boolean): void
  (e: 'menu-click', key: string): void
  (e: 'open-change', keys: string[]): void
}>()

const siderStyle = computed(() => ({
  '--primary-color': props.primaryColor,
}))

function handleMenuClick(info: Parameters<NonNullable<MenuProps['onClick']>>[0]) {
  emit('menu-click', String(info.key))
}

function handleOpenChange(keys: string[]) {
  emit('open-change', keys)
}
</script>

<style scoped>
.layout-sider {
  background: var(--primary-color);
  box-shadow: 2px 0 8px rgba(0, 0, 0, 0.15);
  z-index: 9;
}

.layout-sider :deep(.ant-layout-sider-trigger) {
  background: var(--primary-color);
  filter: brightness(0.95);
}

.layout-menu {
  background: transparent;
  border-right: 0;
}

.layout-menu :deep(.ant-menu-item),
.layout-menu :deep(.ant-menu-submenu-title) {
  color: rgba(255, 255, 255, 0.85);
  margin: 4px 8px;
  width: calc(100% - 16px);
  border-radius: 6px;
}

.ant-layout-sider-collapsed .layout-menu :deep(.ant-menu-item),
.ant-layout-sider-collapsed .layout-menu :deep(.ant-menu-submenu-title) {
  padding: 0;
  margin: 4px auto;
  width: 44px;
  height: 44px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.ant-layout-sider-collapsed .layout-menu :deep(.ant-menu-item-icon) {
  margin: 0;
  font-size: 18px;
}

.ant-layout-sider-collapsed .layout-menu :deep(.ant-menu-title-content) {
  display: none;
}

.layout-menu :deep(.ant-menu-item:hover),
.layout-menu :deep(.ant-menu-submenu-title:hover) {
  background: rgba(255, 255, 255, 0.15);
  color: #fff;
}

.layout-menu :deep(.ant-menu-item-selected) {
  background: rgba(255, 255, 255, 0.25);
  color: #fff;
}

.layout-menu :deep(.ant-menu-submenu-selected > .ant-menu-submenu-title) {
  color: #fff;
}

.layout-menu :deep(.ant-menu-item-icon),
.layout-menu :deep(.ant-menu-submenu-arrow) {
  color: rgba(255, 255, 255, 0.85);
  transition: color 0.3s;
}

.layout-menu :deep(.ant-menu-item:hover .ant-menu-item-icon),
.layout-menu :deep(.ant-menu-item-selected .ant-menu-item-icon) {
  color: #fff;
}

.layout-menu :deep(.ant-menu-sub) {
  background: rgba(0, 0, 0, 0.15);
}
</style>
