import { createApp } from 'vue'
import { createPinia } from 'pinia'
import { useAuthStore } from './stores/auth'

import App from './App.vue'
import router from './router'

const app = createApp(App)

app.use(createPinia())

// 在挂载应用前初始化认证状态
const authStore = useAuthStore()
authStore.initializeAuth().then(() => {
  app.use(router)
  app.mount('#app')
})
