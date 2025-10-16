/// <reference types="vite/client" />
import 'pinia-plugin-persistedstate'

/**
 * 环境变量类型声明
 */
interface ImportMetaEnv {
  /**
   * API 基础 URL
   * @default 'http://localhost:5000/api'
   */
  readonly VITE_API_BASE_URL: string

  /**
   * 应用标题
   * @default 'My API Web'
   */
  readonly VITE_APP_TITLE: string

  /**
   * 应用版本号
   * @optional
   */
  readonly VITE_APP_VERSION?: string

  /**
   * 是否启用 Mock 数据
   * @optional
   */
  readonly VITE_ENABLE_MOCK?: string

  /**
   * Sentry DSN (错误监控)
   * @optional
   */
  readonly VITE_SENTRY_DSN?: string

  /**
   * 环境模式
   */
  readonly MODE: 'development' | 'staging' | 'production'

  /**
   * 是否为开发环境
   */
  readonly DEV: boolean

  /**
   * 是否为生产环境
   */
  readonly PROD: boolean

  /**
   * 是否为服务端渲染
   */
  readonly SSR: boolean

  /**
   * 基础路径
   */
  readonly BASE_URL: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
