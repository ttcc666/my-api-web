import { cacheConfig } from '@/config'

interface CacheData<T> {
  data: T
  timestamp: number
  expiresIn: number
}

export class CacheManager {
  private static getKey(key: string): string {
    return `cache_${key}`
  }

  static set<T>(key: string, data: T, expiresIn?: number): void {
    const cacheData: CacheData<T> = {
      data,
      timestamp: Date.now(),
      expiresIn: expiresIn ?? 30 * 60 * 1000,
    }
    localStorage.setItem(this.getKey(key), JSON.stringify(cacheData))
  }

  static get<T>(key: string): T | null {
    const item = localStorage.getItem(this.getKey(key))
    if (!item) return null

    try {
      const cacheData: CacheData<T> = JSON.parse(item)
      const isExpired = Date.now() - cacheData.timestamp > cacheData.expiresIn

      if (isExpired) {
        this.remove(key)
        return null
      }

      return cacheData.data
    } catch {
      this.remove(key)
      return null
    }
  }

  static remove(key: string): void {
    localStorage.removeItem(this.getKey(key))
  }

  static clear(): void {
    const keys = Object.keys(localStorage)
    keys.forEach((key) => {
      if (key.startsWith('cache_')) {
        localStorage.removeItem(key)
      }
    })
  }
}

export const permissionCache = {
  get: () => CacheManager.get(cacheConfig.permissionCacheKey),
  set: (data: unknown) =>
    CacheManager.set(cacheConfig.permissionCacheKey, data, cacheConfig.permissionCacheExpiry),
  remove: () => CacheManager.remove(cacheConfig.permissionCacheKey),
}

export const menuCache = {
  get: () => CacheManager.get(cacheConfig.menuCacheKey),
  set: (data: unknown) =>
    CacheManager.set(cacheConfig.menuCacheKey, data, cacheConfig.menuCacheExpiry),
  remove: () => CacheManager.remove(cacheConfig.menuCacheKey),
}
