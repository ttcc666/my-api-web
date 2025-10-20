import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import { useAuthStore } from '@/stores/modules/auth/auth'
import { usePermissionStore } from '@/stores/modules/system/permission'
import { useMenuStore } from '@/stores/modules/system/menu'
import { MenuType, type MenuDto } from '@/types/api'
import { routeConfig } from '@/config'

const APP_LAYOUT_ROUTE_NAME = 'app-layout'

const constantRoutes: RouteRecordRaw[] = [
  {
    path: '/login',
    name: 'login',
    component: () => import('@/views/auth/Login.vue'),
  },
  {
    path: '/register',
    name: 'register',
    component: () => import('@/views/auth/Register.vue'),
  },
  {
    path: '/',
    name: APP_LAYOUT_ROUTE_NAME,
    component: () => import('@/layouts/MainLayout.vue'),
    meta: {
      requiresAuth: true,
    },
    children: [
      {
        path: 'profile',
        name: 'user-profile',
        component: () => import('@/views/common/UserProfile.vue'),
        meta: {
          title: '个人中心',
        },
      },
    ],
  },
  {
    path: '/403',
    name: 'forbidden',
    component: () => import('@/views/common/ForbiddenView.vue'),
  },
  {
    path: '/:pathMatch(.*)*',
    name: 'not-found',
    component: () => import('@/views/common/NotFoundView.vue'),
  },
]

const PUBLIC_PAGE_NAMES = new Set<string>(routeConfig.publicPages)

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: constantRoutes,
})

// 使用 Promise 缓存来管理权限加载，避免并发请求
let permissionPromise: Promise<void> | null = null
let menuPromise: Promise<void> | null = null
let dynamicRoutesInitialized = false
const dynamicRouteNames = new Set<string>()

const viewModules = import.meta.glob('@/views/**/*.vue')
const componentLoaderMap = Object.entries(viewModules).reduce<
  Record<string, () => Promise<unknown>>
>((acc, [path, loader]) => {
  const match = path.match(/\/([^/]+)\.vue$/i)
  if (!match?.[1]) {
    return acc
  }
  const componentKey = match[1].toLowerCase()
  acc[componentKey] = loader as () => Promise<unknown>
  return acc
}, {})

function normalizeComponentKey(name: string) {
  return name.replace(/[-_/]/g, '').toLowerCase()
}

function resolveViewComponent(routeName?: string | null) {
  if (!routeName) {
    return null
  }
  const key = normalizeComponentKey(routeName)
  return componentLoaderMap[key] ?? null
}

function normalizePath(path?: string | null): string {
  if (!path) {
    return ''
  }
  if (path === '/') {
    return '/'
  }
  return `/${path.replace(/^\/+|\/+$/g, '')}`
}

function deriveRoutePath(
  fullPath: string,
  parentFullPath: string,
): { path: string; alias?: string[] } {
  const normalizedFull = normalizePath(fullPath)
  const normalizedParent = normalizePath(parentFullPath)

  if (!normalizedFull || normalizedFull === '/') {
    return {
      path: '',
      alias: ['/'],
    }
  }

  if (!normalizedParent || normalizedParent === '/') {
    return {
      path: normalizedFull.replace(/^\//, ''),
    }
  }

  if (normalizedFull === normalizedParent) {
    return { path: '' }
  }

  if (normalizedFull.startsWith(`${normalizedParent}/`)) {
    const relative = normalizedFull.slice(normalizedParent.length + 1)
    return { path: relative }
  }

  return { path: normalizedFull.replace(/^\//, '') }
}

function buildRoutesFromMenus(
  menus: MenuDto[] | undefined,
  ancestors: MenuDto[] = [],
  parentPath = '/',
): RouteRecordRaw[] {
  if (!menus || menus.length === 0) {
    return []
  }

  const routes: RouteRecordRaw[] = []

  menus.forEach((menu) => {
    if (!menu.isEnabled) {
      return
    }

    if (menu.type === MenuType.Directory) {
      const directoryAncestors = [...ancestors, menu]
      const nextParentPath = menu.routePath ?? parentPath
      routes.push(
        ...buildRoutesFromMenus(menu.children, directoryAncestors, nextParentPath ?? parentPath),
      )
      return
    }

    if (menu.type !== MenuType.Route || !menu.routeName || !menu.routePath) {
      return
    }

    const componentLoader = resolveViewComponent(menu.routeName)
    if (!componentLoader) {
      console.warn(
        `[router] 未找到与菜单 ${menu.title} 匹配的视图组件，routeName=${menu.routeName}`,
      )
      return
    }

    const { path, alias } = deriveRoutePath(menu.routePath, parentPath)
    const breadcrumb = ancestors
      .filter((ancestor) => ancestor.type === MenuType.Directory)
      .map((ancestor) => ({
        title: ancestor.title,
        path: ancestor.routePath ?? undefined,
      }))

    const childRoutes = buildRoutesFromMenus(menu.children, [...ancestors, menu], menu.routePath)

    const route: RouteRecordRaw = {
      path,
      name: menu.routeName,
      component: componentLoader,
      meta: {
        title: menu.title,
        permission: menu.permissionCode ?? undefined,
        breadcrumb: breadcrumb.length > 0 ? breadcrumb : undefined,
        rawPath: menu.routePath,
      },
      ...(alias && { alias }),
      ...(childRoutes.length > 0 && { children: childRoutes }),
    }

    routes.push(route)
  })

  return routes
}

function registerDynamicMenuRoutes(menus: MenuDto[]): boolean {
  let addedRoute = false
  const routes = buildRoutesFromMenus(menus)
  routes.forEach((route) => {
    const name = route.name
    if (!name || typeof name !== 'string') {
      return
    }
    if (router.hasRoute(name)) {
      return
    }
    router.addRoute(APP_LAYOUT_ROUTE_NAME, route)
    dynamicRouteNames.add(name)
    addedRoute = true
  })
  dynamicRoutesInitialized = true
  return addedRoute
}

function resetDynamicMenuRoutes() {
  dynamicRouteNames.forEach((name) => {
    if (router.hasRoute(name)) {
      router.removeRoute(name)
    }
  })
  dynamicRouteNames.clear()
  dynamicRoutesInitialized = false
}

function isPublicRoute(routeName: unknown): boolean {
  return typeof routeName === 'string' && PUBLIC_PAGE_NAMES.has(routeName)
}

async function ensurePermissionsLoaded(
  permissionStore: ReturnType<typeof usePermissionStore>,
): Promise<void> {
  if (permissionStore.permissionsLoaded) {
    return
  }

  if (!permissionPromise) {
    permissionPromise = permissionStore.loadUserPermissions().finally(() => {
      permissionPromise = null
    })
  }

  await permissionPromise
}

async function ensureMenusLoaded(menuStore: ReturnType<typeof useMenuStore>): Promise<void> {
  if (menuStore.loaded) {
    return
  }

  if (!menuPromise) {
    menuPromise = menuStore.ensureLoaded().finally(() => {
      menuPromise = null
    })
  }

  await menuPromise
}

type RouterNavigationGuard = Parameters<typeof router.beforeEach>[0]

export const authNavigationGuard: RouterNavigationGuard = async (to, from, next) => {
  const authStore = useAuthStore()
  const permissionStore = usePermissionStore()
  const menuStore = useMenuStore()
  const isAuthenticated = authStore.isAuthenticated
  const isPublicPage = isPublicRoute(to.name)

  if (!isAuthenticated) {
    if (dynamicRoutesInitialized) {
      resetDynamicMenuRoutes()
    }

    if (isPublicPage) {
      return next()
    }

    return next({ name: 'login', query: { redirect: to.fullPath } })
  }

  if (isPublicPage) {
    return next({ path: routeConfig.homePath })
  }

  try {
    await ensurePermissionsLoaded(permissionStore)
  } catch (error) {
    console.error('路由守卫中加载权限失败:', error)
    authStore.logout()
    return next({ name: 'login', query: { redirect: to.fullPath } })
  }

  try {
    await ensureMenusLoaded(menuStore)
  } catch (error) {
    console.error('路由守卫中加载菜单失败:', error)
    authStore.logout()
    return next({ name: 'login', query: { redirect: to.fullPath } })
  }

  if (!dynamicRoutesInitialized) {
    const menuList = Array.isArray(menuStore.menus) ? menuStore.menus : []
    const addedRoutes = registerDynamicMenuRoutes(menuList)
    if (addedRoutes) {
      return next({ path: to.fullPath, replace: true })
    }
  }

  const requiredPermission = to.meta.permission as string | undefined
  if (requiredPermission && !permissionStore.hasPermission(requiredPermission)) {
    return next({ name: 'forbidden' })
  }

  return next()
}

router.beforeEach(authNavigationGuard)

export function resetNavigationState() {
  permissionPromise = null
  menuPromise = null
  resetDynamicMenuRoutes()
}

export default router
