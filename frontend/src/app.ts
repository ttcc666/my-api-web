import { createApp } from 'vue'
import 'ant-design-vue/dist/reset.css'
import { createPinia } from 'pinia'
import piniaPluginPersistedstate from 'pinia-plugin-persistedstate'
import App from './App.vue'
import router from './router'
import { useAuthStore } from './stores/auth'
import { setupPermissionDirective } from './directives/permission'
import { setupApiInterceptors } from './api/interceptors'

const pinia = createPinia()
pinia.use(piniaPluginPersistedstate)

/**
 * 创建并配置 Vue 应用实例
 */
export function createVueApp() {
  const app = createApp(App)

  // 配置 Pinia 状态管理
  app.use(pinia)

  // 注册自定义指令
  setupPermissionDirective(app)

  return app
}

/**
 * 初始化应用并挂载到 DOM
 */
export async function bootstrapApp() {
  // 创建 Vue 应用实例
  const app = createVueApp()

  // 设置 API 拦截器
  setupApiInterceptors()

  // 必须在 use(router) 之前调用，以确保 authStore 实例存在
  const authStore = useAuthStore()

  try {
    // 在挂载应用前初始化认证状态
    await authStore.initializeAuth()

    // 配置路由
    app.use(router)

    // 挂载应用
    app.mount('#app')

    console.log('应用启动成功')
  } catch (error) {
    console.error('应用启动失败:', error)
    throw error
  }
}
