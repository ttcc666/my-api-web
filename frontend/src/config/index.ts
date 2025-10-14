/**
 * 应用配置管理模块
 * 提供统一的配置访问接口
 */

/**
 * API 配置
 */
export const apiConfig = {
  /**
   * API 基础 URL
   */
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api',

  /**
   * 请求超时时间（毫秒）
   */
  timeout: 10000,

  /**
   * Token 刷新缓冲时间（毫秒）
   * 在 Token 过期前这段时间自动刷新
   */
  tokenRefreshBuffer: 30 * 1000,

  /**
   * 请求重试次数
   */
  retryCount: 3,

  /**
   * 重试延迟（毫秒）
   */
  retryDelay: 1000,
} as const

/**
 * 应用配置
 */
export const appConfig = {
  /**
   * 应用标题
   */
  title: import.meta.env.VITE_APP_TITLE || 'My API Web',

  /**
   * 应用版本
   */
  version: import.meta.env.VITE_APP_VERSION || '1.0.0',

  /**
   * 是否启用 Mock 数据
   */
  enableMock: import.meta.env.VITE_ENABLE_MOCK === 'true',

  /**
   * 是否为开发环境
   */
  isDev: import.meta.env.DEV,

  /**
   * 是否为生产环境
   */
  isProd: import.meta.env.PROD,

  /**
   * 环境模式
   */
  mode: import.meta.env.MODE,
} as const

/**
 * 存储配置
 */
export const storageConfig = {
  /**
   * LocalStorage Key 前缀
   */
  prefix: 'app_',

  /**
   * Token 存储 Key
   */
  tokenKey: 'accessToken',

  /**
   * RefreshToken 存储 Key
   */
  refreshTokenKey: 'refreshToken',

  /**
   * 用户信息存储 Key
   */
  userKey: 'user',

  /**
   * 是否启用加密存储
   */
  enableEncryption: import.meta.env.PROD,
} as const

/**
 * 路由配置
 */
export const routeConfig = {
  /**
   * 默认首页路径
   */
  homePath: '/',

  /**
   * 登录页路径
   */
  loginPath: '/login',

  /**
   * 403 页面路径
   */
  forbiddenPath: '/403',

  /**
   * 404 页面路径
   */
  notFoundPath: '/404',

  /**
   * 公共页面（无需登录）
   */
  publicPages: ['login', 'register'],
} as const

/**
 * 分页配置
 */
export const paginationConfig = {
  /**
   * 默认每页条数
   */
  pageSize: 10,

  /**
   * 每页条数选项
   */
  pageSizeOptions: [10, 20, 50, 100],

  /**
   * 是否显示快速跳转
   */
  showQuickJumper: true,

  /**
   * 是否显示总数
   */
  showSizeChanger: true,
} as const

/**
 * UI 配置
 */
export const uiConfig = {
  /**
   * 消息显示时长（毫秒）
   */
  messageDuration: 3000,

  /**
   * 通知显示时长（毫秒）
   */
  notificationDuration: 5000,

  /**
   * 表格最大高度
   */
  tableMaxHeight: 600,

  /**
   * 默认主题
   */
  defaultTheme: 'light' as 'light' | 'dark',
} as const

/**
 * 监控配置
 */
export const monitorConfig = {
  /**
   * Sentry DSN
   */
  sentryDsn: import.meta.env.VITE_SENTRY_DSN,

  /**
   * 是否启用性能监控
   */
  enablePerformanceMonitor: import.meta.env.PROD,

  /**
   * 采样率
   */
  sampleRate: 0.1,
} as const

/**
 * 获取完整的存储 Key
 */
export function getStorageKey(key: string): string {
  return `${storageConfig.prefix}${key}`
}

/**
 * 导出默认配置对象
 */
export default {
  api: apiConfig,
  app: appConfig,
  storage: storageConfig,
  route: routeConfig,
  pagination: paginationConfig,
  ui: uiConfig,
  monitor: monitorConfig,
}
