<template>
  <a-tabs
    type="editable-card"
    size="small"
    hideAdd
    :activeKey="activeKey"
    @change="handleChange"
    @edit="handleEdit"
    class="app-menu-tabs"
  >
    <a-tab-pane v-for="tab in tabs" :key="tab.key" :closable="tab.closable">
      <template #tab>
        <a-dropdown :trigger="['contextmenu']">
          <span>{{ tab.label }}</span>
          <template #overlay>
            <a-menu @click="({ key }: { key: string }) => handleContextMenuClick(tab.key, key)">
              <a-menu-item key="close">关闭当前</a-menu-item>
              <a-menu-item key="closeOthers">关闭其他</a-menu-item>
              <a-menu-item key="closeAll">关闭所有</a-menu-item>
            </a-menu>
          </template>
        </a-dropdown>
      </template>
    </a-tab-pane>
  </a-tabs>
</template>

<script setup lang="ts">
import { storeToRefs } from 'pinia'
import { useRouter } from 'vue-router'
import { useTabStore } from '@/stores/modules/common/tabs'
import { useThemeStore } from '@/stores/modules/common/theme'

const router = useRouter()
const tabStore = useTabStore()
const themeStore = useThemeStore()
const { tabs, activeKey } = storeToRefs(tabStore)

async function navigateTo(path: string) {
  if (router.currentRoute.value.fullPath !== path) {
    await router.push(path)
  }
}

async function handleChange(value: string) {
  const key = String(value)
  tabStore.activate(key)
  const target = tabs.value.find((item) => item.key === key)
  if (target) {
    await navigateTo(target.path)
  }
}

async function handleEdit(targetKey: string | MouseEvent, action: 'add' | 'remove') {
  if (action !== 'remove') {
    return
  }
  const key = typeof targetKey === 'string' ? targetKey : ''
  if (!key) {
    return
  }
  const next = tabStore.remove(key)
  if (next) {
    await navigateTo(next.path)
  }
}

async function handleContextMenuClick(tabKey: string, actionKey: string | number) {
  switch (actionKey) {
    case 'close':
      handleEdit(tabKey, 'remove')
      break
    case 'closeOthers':
      tabStore.closeOthers(tabKey)
      break
    case 'closeAll':
      tabStore.closeAll()
      break
  }
  const target = tabs.value.find((tab) => tab.key === activeKey.value)
  if (target) {
    await navigateTo(target.path)
  }
}
</script>

<style>
.app-menu-tabs {
  --ant-primary-color: v-bind('themeStore.primaryColor');
  background-color: #f0f2f5;
  padding: 4px 12px 0;
}

.app-menu-tabs .ant-tabs-nav {
  margin: 0 !important;
  border-bottom: none !important;
}

.app-menu-tabs .ant-tabs-tab {
  background-color: #e0e0e0;
  border: none !important;
  border-radius: 8px 8px 0 0 !important;
  margin-right: 4px !important;
  padding: 6px 16px !important;
  transition: all 0.3s;
}

.app-menu-tabs .ant-tabs-tab:hover {
  background-color: #d0d0d0;
}

.app-menu-tabs .ant-tabs-tab-active {
  background-color: #fff !important;
  color: var(--ant-primary-color) !important;
  font-weight: 500;
  box-shadow: 0 -2px 5px rgba(0, 0, 0, 0.05);
}

.app-menu-tabs .ant-tabs-tab-btn {
  transition: color 0.3s;
}

.app-menu-tabs .ant-tabs-tab-remove {
  color: #999;
  transition: all 0.3s;
}

.app-menu-tabs .ant-tabs-tab-remove:hover {
  color: #333;
  transform: scale(1.2);
}

.app-menu-tabs .ant-tabs-tab-active .ant-tabs-tab-remove:hover {
  color: var(--ant-primary-color);
}
</style>
