import type { Ref } from 'vue'

export interface AsyncCacheLoaderOptions<T> {
  /**
   * 缓存有效期（毫秒）
   */
  ttl: number
  /**
   * 上次成功加载时间
   */
  lastLoadedAt: Ref<number | null>
  /**
   * 控制加载状态
   */
  loading?: Ref<boolean>
  /**
   * 判断当前数据是否可用
   */
  isDataReady?: () => boolean
  /**
   * 数据请求函数
   */
  fetcher: () => Promise<T>
  /**
   * 成功回调，用于写入状态
   */
  onSuccess: (data: T) => void
  /**
   * 失败回调，交给调用方处理错误文案等
   */
  onError?: (error: unknown) => void
  /**
   * 标记资源是否已加载成功
   */
  markLoaded?: (loaded: boolean) => void
}

export interface AsyncCacheLoader<T> {
  load: (forceRefresh?: boolean) => Promise<T | void>
  refresh: () => Promise<T | void>
  invalidate: () => void
  isCacheValid: () => boolean
}

/**
 * 创建带并发去重与 TTL 校验的异步资源加载器
 * 适用于菜单、权限等需要缓存控制的场景
 */
export function createAsyncCacheLoader<T>(
  options: AsyncCacheLoaderOptions<T>,
): AsyncCacheLoader<T> {
  let loadPromise: Promise<T> | null = null

  const isCacheValid = () => {
    if (typeof options.isDataReady === 'function' && !options.isDataReady()) {
      return false
    }

    const lastLoadedAt = options.lastLoadedAt.value
    if (!lastLoadedAt) {
      return false
    }

    return Date.now() - lastLoadedAt < options.ttl
  }

  const invalidate = () => {
    options.lastLoadedAt.value = null
    loadPromise = null
    options.markLoaded?.(false)
  }

  const runFetcher = async () => {
    if (options.loading) {
      options.loading.value = true
    }
    options.markLoaded?.(false)

    try {
      const data = await options.fetcher()
      options.onSuccess(data)
      options.lastLoadedAt.value = Date.now()
      options.markLoaded?.(true)
      return data
    } catch (error) {
      options.onError?.(error)
      if (options.markLoaded) {
        options.markLoaded(false)
      }
      throw error
    } finally {
      if (options.loading) {
        options.loading.value = false
      }
      loadPromise = null
    }
  }

  const load = async (forceRefresh = false) => {
    if (!forceRefresh && isCacheValid()) {
      return
    }

    if (loadPromise && !forceRefresh) {
      return loadPromise
    }

    if (forceRefresh) {
      invalidate()
    }

    loadPromise = runFetcher()
    return loadPromise
  }

  const refresh = () => load(true)

  return {
    load,
    refresh,
    invalidate,
    isCacheValid,
  }
}
