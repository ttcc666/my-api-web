import { ref } from 'vue'
import { defineStore } from 'pinia'
import type { RouteLocationNormalizedLoaded } from 'vue-router'

export type TabKey = string

export interface TabItem {
  key: TabKey
  path: string
  label: string
  closable: boolean
}

const HOME_TAB_KEY: TabKey = 'home'

function createHomeTab(): TabItem {
  return {
    key: HOME_TAB_KEY,
    path: '/',
    label: '主页',
    closable: false,
  }
}

export const useTabStore = defineStore(
  'tabs',
  () => {
    const tabs = ref<TabItem[]>([createHomeTab()])
    const activeKey = ref<TabKey>(HOME_TAB_KEY)

    const ensureHomeTab = (): void => {
      if (!tabs.value.length) {
        tabs.value.push(createHomeTab())
      }
      const homeTab = tabs.value.find((item) => item.key === HOME_TAB_KEY)
      if (!homeTab) {
        tabs.value.unshift(createHomeTab())
      }
    }

    const syncWithRoute = (route: RouteLocationNormalizedLoaded): void => {
      const name = route.name
      if (name === null || name === undefined) return

      const key = String(name) as TabKey
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
          closable: key !== HOME_TAB_KEY,
        })
      }
      activeKey.value = key
    }

    const activate = (key: TabKey): void => {
      if (!key) return
      activeKey.value = key
    }

    const remove = (key: TabKey): TabItem | null => {
      if (key === HOME_TAB_KEY) {
        activeKey.value = HOME_TAB_KEY
        ensureHomeTab()
        return tabs.value.find((item) => item.key === HOME_TAB_KEY) ?? null
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

    const reset = (): void => {
      tabs.value = [createHomeTab()]
      activeKey.value = HOME_TAB_KEY
    }

    const closeOthers = (key: TabKey): void => {
      const toClose = tabs.value.filter((tab) => tab.key !== key && tab.key !== HOME_TAB_KEY)
      for (const tab of toClose) {
        remove(tab.key)
      }
      activeKey.value = key
    }

    const closeAll = (): void => {
      const toClose = tabs.value.filter((tab) => tab.key !== HOME_TAB_KEY)
      for (const tab of toClose) {
        remove(tab.key)
      }
      activeKey.value = HOME_TAB_KEY
    }

    ensureHomeTab()

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
  },
  {
    persist: {
      pick: ['tabs', 'activeKey'],
    },
  },
)
