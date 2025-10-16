import axios, { type AxiosError, type InternalAxiosRequestConfig } from 'axios'
import { storeToRefs } from 'pinia'
import type { ApiResponse, TokenPayload } from '@/types/api'
import apiClient from '@/utils/request'
import { useAuthStore } from '@/stores/modules/auth/auth'
import { handleError, ApiError, ErrorType, type ErrorInfo } from '@/utils/errorHandler'
import { apiConfig } from '@/config'

const TOKEN_REFRESH_BUFFER_MS = apiConfig.tokenRefreshBuffer

// Token 刷新队列管理
let isRefreshing = false
let failedQueue: { resolve: (value: unknown) => void; reject: (reason?: unknown) => void }[] = []

const processQueue = (error: Error | null, token: string | null = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error)
    } else {
      prom.resolve(token)
    }
  })
  failedQueue = []
}

async function queueRefresh(authStore: ReturnType<typeof useAuthStore>): Promise<string> {
  if (isRefreshing) {
    return new Promise((resolve, reject) => {
      failedQueue.push({ resolve, reject })
    }).then((token) => {
      if (typeof token !== 'string') {
        throw new Error('刷新队列返回空令牌')
      }
      return token
    })
  }

  isRefreshing = true

  try {
    const newAccessToken = await refreshAccessToken(authStore)
    processQueue(null, newAccessToken)
    return newAccessToken
  } catch (err) {
    processQueue(err as Error, null)
    throw err
  } finally {
    isRefreshing = false
  }
}

async function refreshAccessToken(authStore: ReturnType<typeof useAuthStore>): Promise<string> {
  const { refreshToken } = storeToRefs(authStore)
  const refreshTokenValue = refreshToken.value

  if (!refreshTokenValue) {
    throw new Error('缺少 RefreshToken')
  }

  const { data: response } = await axios.post<ApiResponse<TokenPayload>>(
    '/token/refresh',
    { refreshToken: refreshTokenValue },
    {
      baseURL: apiConfig.baseURL,
    },
  )

  if (!response?.success) {
    throw Object.assign(new Error(response?.message ?? '刷新令牌失败'), {
      code: response?.code,
    })
  }

  const newTokens = response.data

  if (!newTokens?.accessToken || !newTokens.refreshToken) {
    throw new Error('刷新接口返回缺少令牌')
  }

  authStore.setTokens(newTokens.accessToken, newTokens.refreshToken)

  return newTokens.accessToken
}

/**
 * 设置 API 客户端的请求和响应拦截器
 */
export function setupApiInterceptors() {
  // 请求拦截器
  apiClient.interceptors.request.use(
    async (config) => {
      const authStore = useAuthStore()
      const { token } = storeToRefs(authStore)
      let accessToken = token.value

      if (!accessToken) {
        return config
      }

      const headers = (config.headers = config.headers ?? {}) as Record<string, unknown>
      const existingHeader = headers['Authorization'] ?? headers['authorization']
      const hasCustomAuthorization = typeof existingHeader === 'string' && existingHeader.length > 0

      if (hasCustomAuthorization) {
        return config
      }

      if (authStore.shouldRefreshToken(TOKEN_REFRESH_BUFFER_MS)) {
        try {
          accessToken = await queueRefresh(authStore)
        } catch (refreshError) {
          authStore.logout()
          handleError(refreshError)
          return Promise.reject(refreshError)
        }
      }

      headers['Authorization'] = `Bearer ${accessToken}`
      return config
    },
    (error) => Promise.reject(error),
  )

  // 响应拦截器
  apiClient.interceptors.response.use(
    (response) => {
      // 检查响应是否被 `value` 字段包裹
      let res = response.data
      if (res && typeof res.value !== 'undefined') {
        res = res.value
      }

      // 处理标准的 ApiResponse 格式
      if (res && typeof res.success === 'boolean' && res.code !== undefined) {
        if (res.success) {
          // 业务成功，返回数据
          return res.data
        }

        const authStore = useAuthStore()
        const statusCode = Number(res.code)
        const tokenExpiredHeader =
          typeof response.headers?.['token-expired'] === 'string'
            ? response.headers['token-expired']
            : (response.headers?.['Token-Expired'] as string | undefined)

        const errorInfo: ErrorInfo = {
          type:
            statusCode === 401
              ? ErrorType.AUTH
              : statusCode === 403
                ? ErrorType.PERMISSION
                : ErrorType.SERVER,
          status: statusCode,
          message:
            tokenExpiredHeader === 'true'
              ? '登录已过期，请重新登录'
              : res.message || '业务处理失败',
          showMessage: true,
          redirect: statusCode === 401 ? '/login' : statusCode === 403 ? '/403' : undefined,
        }

        if (statusCode === 401) {
          authStore.logout()
        }

        const businessError = new ApiError(errorInfo, {
          code: statusCode,
          payload: res.data,
        })

        handleError(businessError)
        return Promise.reject(businessError)
      }

      // 对于非标准格式的响应，直接返回
      return res
    },
    async (error: AxiosError) => {
      const originalRequest = error.config as
        | (InternalAxiosRequestConfig & { _retry?: boolean })
        | undefined
      const authStore = useAuthStore()

      const headers = error.response?.headers as Record<string, string | undefined> | undefined
      const tokenExpired = headers?.['token-expired'] === 'true'

      if (
        error.response?.status === 401 &&
        tokenExpired &&
        originalRequest &&
        !originalRequest._retry
      ) {
        originalRequest._retry = true

        try {
          const newAccessToken = await queueRefresh(authStore)
          originalRequest.headers = originalRequest.headers ?? {}
          originalRequest.headers['Authorization'] = `Bearer ${newAccessToken}`
          return apiClient(originalRequest)
        } catch (refreshError) {
          authStore.logout()
          handleError(refreshError)
          return Promise.reject(refreshError)
        }
      }

      // 对于非 token 过期的 401 错误，先登出再处理错误
      if (error.response?.status === 401 && !tokenExpired) {
        authStore.logout()
      }

      // 使用统一错误处理系统
      handleError(error)
      return Promise.reject(error.response?.data || error)
    },
  )
}
