import { createApp } from 'vue'
import { createPinia } from 'pinia'
import axios, { type AxiosError, type InternalAxiosRequestConfig } from 'axios'

import App from './App.vue'
import router from './router'
import apiClient from './utils/request'
import { useAuthStore } from './stores/auth'
import { setupPermissionDirective } from './directives/permission'

// --- 依赖注入的核心：在 main.ts 中配置拦截器 ---

// 1. 请求拦截器
apiClient.interceptors.request.use(
  (config) => {
    // 这里可以安全地使用 useAuthStore，因为 Pinia 已经初始化
    const authStore = useAuthStore()
    if (authStore.token && !config.headers['Authorization']) {
      config.headers['Authorization'] = `Bearer ${authStore.token}`
    }
    return config
  },
  (error) => Promise.reject(error)
)

// 2. 响应拦截器 (包含 token 刷新逻辑)
let isRefreshing = false
let failedQueue: { resolve: (value: unknown) => void; reject: (reason?: unknown) => void }[] = []

const processQueue = (error: Error | null, token: string | null = null) => {
  failedQueue.forEach(prom => {
    if (error) {
      prom.reject(error)
    } else {
      prom.resolve(token)
    }
  })
  failedQueue = []
}

apiClient.interceptors.response.use(
  (response) => {
    // 检查响应是否被 `value` 字段包裹
    let res = response.data;
    if (res && typeof res.value !== 'undefined') {
      res = res.value;
    }

    if (res && typeof res.success === 'boolean' && res.code !== undefined) {
      if (res.success) {
        return res.data;
      } else {
        return Promise.reject(new Error(res.message || 'Error'));
      }
    }
    return res;
  },
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean }
    const authStore = useAuthStore()

    if (error.response?.status === 401 && (error.response.headers as { 'token-expired'?: string })['token-expired'] === 'true' && !originalRequest._retry) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject })
        }).then(token => {
          if (originalRequest.headers) {
            originalRequest.headers['Authorization'] = 'Bearer ' + token
          }
          return apiClient(originalRequest)
        })
      }

      originalRequest._retry = true
      isRefreshing = true

      try {
        const { refreshToken } = authStore
        if (!refreshToken) {
          authStore.logout()
          router.push('/login')
          return Promise.reject(error)
        }

        // 注意：这里我们使用原始的 axios 来刷新 token，以避免触发自身的拦截器导致死循环
        const res = await axios.post<{ accessToken: string; refreshToken: string }>('/token/refresh', { refreshToken }, {
          baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api',
        })
        
        const newTokens = res.data
        authStore.setTokens(newTokens.accessToken, newTokens.refreshToken)
        
        if (originalRequest.headers) {
          originalRequest.headers['Authorization'] = `Bearer ${newTokens.accessToken}`
        }
        
        processQueue(null, newTokens.accessToken)
        return apiClient(originalRequest)

      } catch (refreshError) {
        processQueue(refreshError as Error, null)
        authStore.logout()
        router.push('/login')
        return Promise.reject(refreshError)
      } finally {
        isRefreshing = false
      }
    }
    
    if (error.response?.status === 403) {
      router.push('/403');
    }

    return Promise.reject(error.response?.data || error)
  }
)

// --- Vue 应用初始化 ---

const app = createApp(App)

app.use(createPinia())

// 必须在 use(router) 之前调用，以确保 authStore 实例存在
const authStore = useAuthStore()

// 注册自定义指令
setupPermissionDirective(app)

// 在挂载应用前初始化认证状态
authStore.initializeAuth().then(() => {
  app.use(router)
  app.mount('#app')
})
