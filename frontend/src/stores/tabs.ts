import { ref } from 'vue'
import { defineStore } from 'pinia'
import type { RouteLocationNormalizedLoaded } from 'vue-router'

interface TabItem {
  key: string
  path: string
  label: string
  closable: boolean
}

function createHomeTab(): TabItem {
  return {
    key: 'home',
    path: '/',
    label: '主页',
    closable: false,
  }
}

export const useTabStore = defineStore('tabs', () => {
  const tabs = ref<TabItem[]>([createHomeTab()])
  const activeKey = ref<string>('home')

  const ensureHomeTab = () => {
    if (!tabs.value.length) {
      tabs.value.push(createHomeTab())
    }
    const homeTab = tabs.value.find((item) => item.key === 'home')
    if (!homeTab) {
      tabs.value.unshift(createHomeTab())
    }
  }

  const syncWithRoute = (route: RouteLocationNormalizedLoaded) => {
    const name = route.name
    if (name === null || name === undefined) return
    const key = String(name)

    const label = (route.meta?.title as string) || ''
    if (!label) return

    ensureHomeTab()

    const existing = tabs.value.find((item) => item.key === key)
    if (existing) {
      existing.path = route.fullPath
      existing.label = label
    } else {
      tabs.value.push({
        key,
        path: route.fullPath,
        label,
        closable: key !== 'home',
      })
    }
    activeKey.value = key
  }

  const activate = (key: string) => {
    if (!key) return
    activeKey.value = key
  }

  const remove = (key: string): TabItem | null => {
    if (key === 'home') {
      activeKey.value = 'home'
      ensureHomeTab()
      return tabs.value.find((item) => item.key === 'home') || null
    }

    const index = tabs.value.findIndex((item) => item.key === key)
    if (index === -1) return null

    const isActive = activeKey.value === key
    tabs.value.splice(index, 1)

    if (!tabs.value.length) {
      const homeTab = createHomeTab()
      tabs.value.push(homeTab)
      activeKey.value = homeTab.key
      return homeTab
    }

    if (isActive) {
      const candidate = tabs.value[index] ?? tabs.value[index - 1] ?? tabs.value[0]
      if (candidate) {
        activeKey.value = candidate.key
        return candidate
      }

      const homeTab = createHomeTab()
      tabs.value.push(homeTab)
      activeKey.value = homeTab.key
      return homeTab
    }

    return null
  }

  const reset = () => {
    tabs.value = [createHomeTab()]
    activeKey.value = 'home'
  }

  const closeOthers = (key: string) => {
    const toClose = tabs.value.filter(tab => tab.key !== key && tab.key !== 'home')
    for (const tab of toClose) {
      remove(tab.key)
    }
    activeKey.value = key
  }

  const closeAll = () => {
    const toClose = tabs.value.filter(tab => tab.key !== 'home')
    for (const tab of toClose) {
      remove(tab.key)
    }
    activeKey.value = 'home'
  }

  return {
    tabs,
    activeKey,
    syncWithRoute,
    activate,
    remove,
    reset,
    closeOthers,
    closeAll,
  }
})
