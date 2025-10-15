import { message, notification } from '@/plugins/antd'
import router from '@/router'
import type { AxiosError } from 'axios'

// 错误类型枚举
export enum ErrorType {
  NETWORK = 'NETWORK',
  TIMEOUT = 'TIMEOUT',
  SERVER = 'SERVER',
  CLIENT = 'CLIENT',
  AUTH = 'AUTH',
  PERMISSION = 'PERMISSION',
  RATE_LIMIT = 'RATE_LIMIT',
  UNKNOWN = 'UNKNOWN',
}

// 错误信息接口
export interface ErrorInfo {
  type: ErrorType
  status?: number
  message: string
  title?: string
  showNotification?: boolean
  showMessage?: boolean
  redirect?: string
}

// 错误处理选项
export interface ErrorHandlerOptions {
  silent?: boolean // 静默处理，不显示任何提示
  skipRedirect?: boolean // 跳过自动跳转
  customMessage?: string // 自定义错误消息
}

// HTTP 状态码错误映射
const HTTP_ERROR_MAP: Record<number, ErrorInfo> = {
  400: {
    type: ErrorType.CLIENT,
    status: 400,
    title: '请求错误',
    message: '请求参数有误，请检查后重试',
    showMessage: true,
  },
  401: {
    type: ErrorType.AUTH,
    status: 401,
    title: '认证失败',
    message: '登录已过期，请重新登录',
    showMessage: true,
    redirect: '/login',
  },
  403: {
    type: ErrorType.PERMISSION,
    status: 403,
    title: '权限不足',
    message: '您没有权限访问此资源',
    showMessage: true,
    redirect: '/403',
  },
  404: {
    type: ErrorType.CLIENT,
    status: 404,
    title: '资源不存在',
    message: '请求的资源不存在',
    showMessage: true,
  },
  408: {
    type: ErrorType.TIMEOUT,
    status: 408,
    title: '请求超时',
    message: '请求超时，请检查网络连接后重试',
    showMessage: true,
  },
  429: {
    type: ErrorType.RATE_LIMIT,
    status: 429,
    title: '请求过于频繁',
    message: '请求过于频繁，请稍后再试',
    showMessage: true,
  },
  500: {
    type: ErrorType.SERVER,
    status: 500,
    title: '服务器错误',
    message: '服务器内部错误，请稍后重试',
    showNotification: true,
  },
  502: {
    type: ErrorType.SERVER,
    status: 502,
    title: '网关错误',
    message: '服务器网关错误，请稍后重试',
    showNotification: true,
  },
  503: {
    type: ErrorType.SERVER,
    status: 503,
    title: '服务不可用',
    message: '服务暂时不可用，请稍后重试',
    showNotification: true,
  },
  504: {
    type: ErrorType.SERVER,
    status: 504,
    title: '网关超时',
    message: '服务器网关超时，请稍后重试',
    showNotification: true,
  },
}

// 类型守卫函数
function isAxiosError(error: unknown): error is AxiosError {
  return typeof error === 'object' && error !== null && 'isAxiosError' in error
}

function hasProperty<T extends string>(obj: unknown, prop: T): obj is Record<T, unknown> {
  return typeof obj === 'object' && obj !== null && prop in obj
}

function isErrorWithMessage(error: unknown): error is { message: string } {
  return hasProperty(error, 'message') && typeof error.message === 'string'
}

function isErrorWithCode(error: unknown): error is { code: string } {
  return hasProperty(error, 'code') && typeof error.code === 'string'
}

/**
 * 检查网络连接状态
 */
export function isNetworkError(error: unknown): boolean {
  if (isAxiosError(error)) {
    return (
      !error.response &&
      (error.code === 'NETWORK_ERROR' ||
        error.code === 'ERR_NETWORK' ||
        error.message?.includes('Network Error') ||
        !navigator.onLine)
    )
  }

  if (isErrorWithCode(error)) {
    return error.code === 'NETWORK_ERROR' || error.code === 'ERR_NETWORK'
  }

  if (isErrorWithMessage(error)) {
    return error.message.includes('Network Error')
  }

  return !navigator.onLine
}

/**
 * 检查是否为超时错误
 */
export function isTimeoutError(error: unknown): boolean {
  if (isErrorWithCode(error)) {
    return error.code === 'ECONNABORTED' || error.code === 'TIMEOUT'
  }

  if (isErrorWithMessage(error)) {
    return error.message.includes('timeout')
  }

  return false
}

/**
 * 获取错误信息
 */
export function getErrorInfo(error: unknown): ErrorInfo {
  if (error instanceof ApiError) {
    return {
      type: error.type,
      status: error.status,
      title: error.title,
      message: error.message,
      showMessage: error.showMessage,
      showNotification: error.showNotification,
      redirect: error.redirect,
    }
  }

  // 网络连接错误
  if (isNetworkError(error)) {
    return {
      type: ErrorType.NETWORK,
      title: '网络连接失败',
      message: '网络连接失败，请检查网络设置',
      showNotification: true,
    }
  }

  // 超时错误
  if (isTimeoutError(error)) {
    return {
      type: ErrorType.TIMEOUT,
      title: '请求超时',
      message: '请求超时，请检查网络连接后重试',
      showMessage: true,
    }
  }

  // Axios 错误处理
  if (isAxiosError(error)) {
    const status = error.response?.status
    if (status && HTTP_ERROR_MAP[status]) {
      const errorInfo = { ...HTTP_ERROR_MAP[status] }

      // 如果服务器返回了自定义错误消息，使用服务器消息
      const responseData = error.response?.data
      if (responseData && typeof responseData === 'object' && 'message' in responseData) {
        const serverMessage = (responseData as { message: unknown }).message
        if (typeof serverMessage === 'string') {
          errorInfo.message = serverMessage
        }
      }

      return errorInfo
    }

    // 5xx 服务器错误
    if (status && status >= 500) {
      return {
        type: ErrorType.SERVER,
        status,
        title: '服务器错误',
        message: `服务器错误 (${status})，请稍后重试`,
        showNotification: true,
      }
    }

    // 4xx 客户端错误
    if (status && status >= 400 && status < 500) {
      return {
        type: ErrorType.CLIENT,
        status,
        title: '请求错误',
        message: `请求错误 (${status})，请检查请求参数`,
        showMessage: true,
      }
    }
  }

  // 未知错误
  const message = isErrorWithMessage(error) ? error.message : '发生未知错误，请稍后重试'
  return {
    type: ErrorType.UNKNOWN,
    title: '未知错误',
    message,
    showMessage: true,
  }
}

/**
 * 处理错误并显示相应的提示
 * @param error 错误对象
 * @param options 错误处理选项
 */
export function handleError(error: unknown, options?: ErrorHandlerOptions): void {
  const errorInfo = getErrorInfo(error)

  // 静默模式：只记录日志，不显示提示和跳转
  if (options?.silent) {
    console.error('API Error (Silent):', {
      type: errorInfo.type,
      status: errorInfo.status,
      message: errorInfo.message,
      originalError: error,
    })
    return
  }

  console.error('API Error:', {
    type: errorInfo.type,
    status: errorInfo.status,
    message: errorInfo.message,
    originalError: error,
  })

  // 使用自定义消息（如果提供）
  const displayMessage = options?.customMessage || errorInfo.message

  // 显示错误提示
  if (errorInfo.showMessage) {
    message.error(displayMessage)
  } else if (errorInfo.showNotification) {
    notification.error({
      message: errorInfo.title || '错误',
      description: displayMessage,
      duration: 5,
    })
  }

  // 处理页面跳转（除非明确跳过）
  if (errorInfo.redirect && !options?.skipRedirect) {
    router.push(errorInfo.redirect)
  }
}

/**
 * 创建自定义错误类
 */
export class ApiError extends Error {
  public readonly type: ErrorType
  public readonly status?: number
  public readonly originalError: unknown
  public readonly title?: string
  public readonly showMessage?: boolean
  public readonly showNotification?: boolean
  public readonly redirect?: string

  constructor(errorInfo: ErrorInfo, originalError?: unknown) {
    super(errorInfo.message)
    this.name = 'ApiError'
    this.type = errorInfo.type
    this.status = errorInfo.status
    this.originalError = originalError
    this.title = errorInfo.title
    this.showMessage = errorInfo.showMessage
    this.showNotification = errorInfo.showNotification
    this.redirect = errorInfo.redirect
  }
}
