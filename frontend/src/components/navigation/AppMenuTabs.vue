<template>
  <n-tabs
    type="card"
    size="small"
    :value="activeKey"
    @update:value="handleUpdateValue"
    closable
    @close="handleClose"
    class="app-menu-tabs"
  >
    <n-tab-pane
      v-for="tab in tabs"
      :key="tab.key"
      :name="tab.key"
      :tab="tab.label"
      :closable="tab.closable"
    />
  </n-tabs>
</template>

<script setup lang="ts">
import { storeToRefs } from 'pinia'
import { useRouter } from 'vue-router'
import { useTabStore } from '@/stores/tabs'

const router = useRouter()
const tabStore = useTabStore()
const { tabs, activeKey } = storeToRefs(tabStore)

const navigateTo = async (path: string) => {
  if (router.currentRoute.value.fullPath !== path) {
    await router.push(path)
  }
}

const handleUpdateValue = async (value: string | number) => {
  const key = String(value)
  tabStore.activate(key)
  const target = tabs.value.find((item) => item.key === key)
  if (target) {
    await navigateTo(target.path)
  }
}

const handleClose = async (value: string | number) => {
  const key = String(value)
  const next = tabStore.remove(key)
  if (next) {
    await navigateTo(next.path)
  }
}
</script>
