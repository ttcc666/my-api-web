import { describe, it, expect, beforeEach, vi } from 'vitest'
import type {
  NavigationGuardNext,
  RouteLocationNormalized,
  RouteLocationNormalizedLoaded,
} from 'vue-router'

let authStore: {
  isAuthenticated: boolean
  logout: () => void
}
let permissionStore: {
  permissionsLoaded: boolean
  loadUserPermissions: (forceRefresh?: boolean) => Promise<void>
  hasPermission: (permission: string) => boolean
}
let menuStore: {
  loaded: boolean
  menus: unknown[]
  ensureLoaded: () => Promise<void>
  refreshMenus: () => Promise<void>
}

vi.mock('@/stores/modules/auth/auth', () => ({
  useAuthStore: () => authStore,
}))

vi.mock('@/stores/modules/system/permission', () => ({
  usePermissionStore: () => permissionStore,
}))

vi.mock('@/stores/modules/system/menu', () => ({
  useMenuStore: () => menuStore,
}))

beforeEach(() => {
  vi.resetModules()

  authStore = {
    isAuthenticated: false,
    logout: vi.fn(),
  }

  permissionStore = {
    permissionsLoaded: false,
    loadUserPermissions: vi.fn(async () => {
      permissionStore.permissionsLoaded = true
    }),
    hasPermission: vi.fn(() => true),
  }

  menuStore = {
    loaded: false,
    menus: [],
    ensureLoaded: vi.fn(async () => {
      menuStore.loaded = true
    }),
    refreshMenus: vi.fn(async () => {
      menuStore.loaded = false
      await menuStore.ensureLoaded()
    }),
  }
})

async function loadGuard() {
  const module = await import('@/router/index')
  module.resetNavigationState()
  return module.authNavigationGuard
}

function createRoute(
  name: string,
  fullPath: string,
  meta: Record<string, unknown> = {},
): RouteLocationNormalizedLoaded {
  return {
    name,
    fullPath,
    path: fullPath,
    meta,
    hash: '',
    query: {},
    params: {},
    redirectedFrom: undefined,
    matched: [],
  } as RouteLocationNormalizedLoaded
}

function createNextRecorder() {
  const calls: unknown[] = []
  const next: NavigationGuardNext = (arg?: unknown) => {
    calls.push(arg)
  }
  return { next, calls }
}

describe('router navigation guard', () => {
  it('redirects guests to login for protected routes', async () => {
    authStore.isAuthenticated = false
    const guard = await loadGuard()
    const { next, calls } = createNextRecorder()

    await guard.call(
      undefined,
      createRoute('app-layout', '/') as RouteLocationNormalized,
      createRoute('home', '/'),
      next,
    )

    expect(calls[0]).toEqual({ name: 'login', query: { redirect: '/' } })
  })

  it('redirects authenticated users away from public pages', async () => {
    authStore.isAuthenticated = true
    permissionStore.permissionsLoaded = true
    menuStore.loaded = true

    const guard = await loadGuard()
    const { next, calls } = createNextRecorder()

    await guard.call(
      undefined,
      createRoute('login', '/login') as RouteLocationNormalized,
      createRoute('app-layout', '/'),
      next,
    )

    expect(calls[0]).toEqual({ path: '/' })
  })

  it('blocks access when permission is missing', async () => {
    authStore.isAuthenticated = true
    permissionStore.permissionsLoaded = true
    menuStore.loaded = true
    permissionStore.hasPermission = vi.fn(() => false)

    const guard = await loadGuard()
    const { next, calls } = createNextRecorder()

    await guard.call(
      undefined,
      createRoute('secured', '/secured', { permission: 'admin' }) as RouteLocationNormalized,
      createRoute('app-layout', '/'),
      next,
    )

    expect(calls[0]).toEqual({ name: 'forbidden' })
  })

  it('allows access when all checks pass', async () => {
    authStore.isAuthenticated = true
    permissionStore.permissionsLoaded = true
    menuStore.loaded = true
    permissionStore.hasPermission = vi.fn(() => true)

    const guard = await loadGuard()
    const { next, calls } = createNextRecorder()

    await guard.call(
      undefined,
      createRoute('dashboard', '/dashboard') as RouteLocationNormalized,
      createRoute('app-layout', '/'),
      next,
    )

    expect(calls.length).toBe(1)
    expect(calls[0]).toBeUndefined()
  })
})
