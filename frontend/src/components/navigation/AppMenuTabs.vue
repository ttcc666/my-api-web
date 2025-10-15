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
    <a-tab-pane
      v-for="tab in tabs"
      :key="tab.key"
      :tab="tab.label"
      :closable="tab.closable"
    />
  </a-tabs>
</template>

<script setup lang="ts">
import { storeToRefs } from 'pinia'
import { useRouter } from 'vue-router'
import { useTabStore } from '@/stores/tabs'

const router = useRouter()
const tabStore = useTabStore()
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
</script>
