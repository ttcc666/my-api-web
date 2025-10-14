import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { usePermissionStore } from '@/stores/permission'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
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
      component: () => import('@/layouts/MainLayout.vue'),
      children: [
        {
          path: '',
          name: 'home',
          component: () => import('@/views/Home.vue'),
          meta: {
            permission: 'dashboard:view',
            title: '主页',
          },
        },
        // 在此添加其他需要身份验证的路由
        {
          path: '/admin/roles',
          name: 'role-management',
          component: () => import('@/views/admin/RoleManagement.vue'),
          meta: {
            permission: 'role:view',
            title: '角色管理',
            breadcrumb: [
              {
                title: '系统管理',
              },
            ],
          },
        },
        {
          path: '/admin/users',
          name: 'user-management',
          component: () => import('@/views/admin/UserManagement.vue'),
          meta: {
            permission: 'user:view',
            title: '用户管理',
            breadcrumb: [
              {
                title: '系统管理',
              },
            ],
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
  ],
})

// 使用 Promise 缓存来管理权限加载，避免并发请求
let permissionPromise: Promise<void> | null = null

router.beforeEach(async (to, from, next) => {
  const authStore = useAuthStore()
  const permissionStore = usePermissionStore()
  const isAuthenticated = authStore.isAuthenticated
  const publicPages = ['login', 'register']
  const isPublicPage = publicPages.includes(to.name as string)

  // 1. 如果已登录
  if (isAuthenticated) {
    // 1.1 如果要去的是公共页面（如登录页），则重定向到首页
    if (isPublicPage) {
      return next({ name: 'home' })
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

    // 1.3 检查页面权限
    const requiredPermission = to.meta.permission as string | undefined
    if (requiredPermission && !permissionStore.hasPermission(requiredPermission)) {
      // 如果需要权限但用户没有，则跳转到 403 页面
      return next({ name: 'forbidden' })
    }

    // 1.4 所有检查通过，放行
    return next()
  }
  // 2. 如果未登录
  else {
    // 2.1 如果要去的是公共页面，则直接放行
    if (isPublicPage) {
      return next()
    }
    // 2.2 如果要去的是私有页面，则重定向到登录页
    return next({ name: 'login', query: { redirect: to.fullPath } })
  }
})

export default router
