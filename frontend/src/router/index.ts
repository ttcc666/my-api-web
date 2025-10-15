import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { usePermissionStore } from '@/stores/permission'
import { useMenuStore } from '@/stores/menu'
import { MenuType, type MenuDto } from '@/types/api'

const APP_LAYOUT_ROUTE_NAME = 'app-layout'

const constantRoutes: RouteRecordRaw[] = [
  {
    path: '/login',
    name: 'login',
    component: () => import('@/views/Login.vue'),
  },
  {
    path: '/register',
    name: 'register',
    component: () => import('@/views/Register.vue'),
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
        component: () => import('@/views/profile/UserProfile.vue'),
        meta: {
          title: '个人中心',
        },
      },
    ],
  },
  {
    path: '/403',
    name: 'forbidden',
    component: () => import('@/views/ForbiddenView.vue'),
  },
  {
    path: '/:pathMatch(.*)*',
    name: 'not-found',
    component: () => import('@/views/NotFoundView.vue'),
  },
]

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

function registerDynamicMenuRoutes(menus: MenuDto[]) {
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
  })
  dynamicRoutesInitialized = true
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

router.beforeEach(async (to, from, next) => {
  const authStore = useAuthStore()
  const permissionStore = usePermissionStore()
  const menuStore = useMenuStore()
  const isAuthenticated = authStore.isAuthenticated
  const publicPages = ['login', 'register']
  const isPublicPage = publicPages.includes(to.name as string)

  // 1. 如果已登录
  if (isAuthenticated) {
    // 1.1 如果要去的是公共页面（如登录页），则重定向到首页
    if (isPublicPage) {
      return next({ path: '/' })
    }

    // 1.2 如果权限未加载，则加载权限
    if (!permissionStore.permissionsLoaded) {
      // 如果没有正在进行的权限加载，创建新的 Promise
      if (!permissionPromise) {
        permissionPromise = permissionStore.loadUserPermissions().finally(() => {
          // 加载完成后清除 Promise 缓存
          permissionPromise = null
        })
      }

      try {
        // 等待权限加载完成（可能是当前创建的，也可能是之前创建的）
        await permissionPromise
      } catch (error) {
        console.error('路由守卫中加载权限失败:', error)
        authStore.logout()
        return next({ name: 'login', query: { redirect: to.fullPath } })
      }
    }

    // 1.3 如果菜单未加载，则加载菜单
    if (!menuStore.loaded) {
      if (!menuPromise) {
        menuPromise = menuStore.ensureLoaded().finally(() => {
          menuPromise = null
        })
      }

      try {
        await menuPromise
      } catch (error) {
        console.error('路由守卫中加载菜单失败:', error)
        authStore.logout()
        return next({ name: 'login', query: { redirect: to.fullPath } })
      }
    }

    // 1.4 注册动态路由
    if (!dynamicRoutesInitialized) {
      const menuList = Array.isArray(menuStore.menus) ? menuStore.menus : []
      registerDynamicMenuRoutes(menuList)
      return next(to.fullPath)
    }

    // 1.4 检查页面权限
    const requiredPermission = to.meta.permission as string | undefined
    if (requiredPermission && !permissionStore.hasPermission(requiredPermission)) {
      // 如果需要权限但用户没有，则跳转到 403 页面
      return next({ name: 'forbidden' })
    }

    // 1.5 所有检查通过，放行
    return next()
  }
  // 2. 如果未登录
  else {
    if (dynamicRoutesInitialized) {
      resetDynamicMenuRoutes()
    }
    // 2.1 如果要去的是公共页面，则直接放行
    if (isPublicPage) {
      return next()
    }
    // 2.2 如果要去的是私有页面，则重定向到登录页
    return next({ name: 'login', query: { redirect: to.fullPath } })
  }
})

export default router
