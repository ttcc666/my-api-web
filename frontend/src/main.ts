import { bootstrapApp } from './app'

// 启动应用
bootstrapApp().catch((error) => {
  console.error('应用启动失败:', error)
})
