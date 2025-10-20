import { beforeEach, describe, expect, it } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import type { RouteLocationNormalizedLoaded } from 'vue-router'
import { useTabStore } from '@/stores/modules/common/tabs'

function createRoute(name: string, fullPath: string, title: string): RouteLocationNormalizedLoaded {
  return {
    name,
    fullPath,
    path: fullPath,
    meta: {
      title,
    },
    hash: '',
    query: {},
    params: {},
    matched: [],
    redirectedFrom: undefined,
  } as unknown as RouteLocationNormalizedLoaded
}

describe('useTabStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('adds and activates tab from route', () => {
    const store = useTabStore()
    const route = createRoute('dashboard', '/dashboard', '仪表盘')

    store.syncWithRoute(route)

    expect(store.tabs).toHaveLength(2)
    expect(store.activeKey).toBe('dashboard')
    const added = store.tabs.find((tab) => tab.key === 'dashboard')
    expect(added).toBeTruthy()
    expect(added?.path).toBe('/dashboard')
    expect(added?.label).toBe('仪表盘')
    expect(added?.closable).toBe(true)
  })

  it('removes active tab and selects previous one', () => {
    const store = useTabStore()
    store.syncWithRoute(createRoute('dashboard', '/dashboard', '仪表盘'))
    store.syncWithRoute(createRoute('reports', '/reports', '报表中心'))

    const removed = store.remove('reports')

    expect(removed?.key).toBe('dashboard')
    expect(store.activeKey).toBe('dashboard')
    expect(store.tabs.map((tab) => tab.key)).toEqual(['home', 'dashboard'])
  })

  it('closes other tabs but keeps target and home', () => {
    const store = useTabStore()
    store.syncWithRoute(createRoute('dashboard', '/dashboard', '仪表盘'))
    store.syncWithRoute(createRoute('reports', '/reports', '报表中心'))

    store.closeOthers('reports')

    expect(store.tabs.map((tab) => tab.key)).toEqual(['home', 'reports'])
    expect(store.activeKey).toBe('reports')
  })

  it('closes all tabs except home', () => {
    const store = useTabStore()
    store.syncWithRoute(createRoute('dashboard', '/dashboard', '仪表盘'))
    store.closeAll()

    expect(store.tabs).toHaveLength(1)
    expect(store.tabs[0]?.key).toBe('home')
    expect(store.activeKey).toBe('home')
  })
})
