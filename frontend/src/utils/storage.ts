/**
 * 安全存储工具类
 * 提供加密的 LocalStorage 操作
 */

import { storageConfig, getStorageKey, appConfig } from '@/config'

/**
 * 简单的 AES 加密实现（基于 Web Crypto API）
 */
class CryptoService {
  private key: CryptoKey | null = null
  private readonly algorithm = 'AES-GCM'
  private readonly keyLength = 256

  /**
   * 初始化加密密钥
   */
  async init(): Promise<void> {
    if (this.key) return

    // 从应用标识生成密钥（生产环境应使用更安全的方式）
    const encoder = new TextEncoder()
    const keyMaterial = encoder.encode(
      `${appConfig.title}-${window.location.hostname}`.padEnd(32, '0').slice(0, 32),
    )

    try {
      this.key = await crypto.subtle.importKey(
        'raw',
        keyMaterial,
        this.algorithm,
        false,
        ['encrypt', 'decrypt'],
      )
    } catch (error) {
      console.warn('加密密钥初始化失败，将使用明文存储:', error)
    }
  }

  /**
   * 加密数据
   */
  async encrypt(data: string): Promise<string> {
    if (!this.key || !storageConfig.enableEncryption) {
      return data
    }

    try {
      const encoder = new TextEncoder()
      const dataBuffer = encoder.encode(data)

      // 生成随机 IV
      const iv = crypto.getRandomValues(new Uint8Array(12))

      const encryptedBuffer = await crypto.subtle.encrypt(
        {
          name: this.algorithm,
          iv,
        },
        this.key,
        dataBuffer,
      )

      // 将 IV 和加密数据合并
      const result = new Uint8Array(iv.length + encryptedBuffer.byteLength)
      result.set(iv, 0)
      result.set(new Uint8Array(encryptedBuffer), iv.length)

      // 转换为 Base64
      return this.arrayBufferToBase64(result)
    } catch (error) {
      console.error('加密失败:', error)
      return data
    }
  }

  /**
   * 解密数据
   */
  async decrypt(encryptedData: string): Promise<string> {
    if (!this.key || !storageConfig.enableEncryption) {
      return encryptedData
    }

    try {
      // 从 Base64 解码
      const encryptedBuffer = this.base64ToArrayBuffer(encryptedData)

      // 提取 IV 和加密数据
      const iv = encryptedBuffer.slice(0, 12)
      const data = encryptedBuffer.slice(12)

      const decryptedBuffer = await crypto.subtle.decrypt(
        {
          name: this.algorithm,
          iv,
        },
        this.key,
        data,
      )

      const decoder = new TextDecoder()
      return decoder.decode(decryptedBuffer)
    } catch (error) {
      console.error('解密失败:', error)
      return encryptedData
    }
  }

  /**
   * ArrayBuffer 转 Base64
   */
  private arrayBufferToBase64(buffer: Uint8Array): string {
    let binary = ''
    for (let i = 0; i < buffer.length; i++) {
      binary += String.fromCharCode(buffer[i]!)
    }
    return btoa(binary)
  }

  /**
   * Base64 转 ArrayBuffer
   */
  private base64ToArrayBuffer(base64: string): Uint8Array {
    const binary = atob(base64)
    const bytes = new Uint8Array(binary.length)
    for (let i = 0; i < binary.length; i++) {
      bytes[i] = binary.charCodeAt(i)
    }
    return bytes
  }
}

// 单例实例
const cryptoService = new CryptoService()

/**
 * 安全存储类
 */
class SecureStorage {
  private initialized = false

  /**
   * 初始化存储服务
   */
  async init(): Promise<void> {
    if (this.initialized) return
    await cryptoService.init()
    this.initialized = true
  }

  /**
   * 设置项
   */
  async setItem(key: string, value: unknown): Promise<void> {
    try {
      await this.init()
      const stringValue = JSON.stringify(value)
      const encryptedValue = await cryptoService.encrypt(stringValue)
      const storageKey = getStorageKey(key)
      localStorage.setItem(storageKey, encryptedValue)
    } catch (error) {
      console.error(`存储数据失败 [${key}]:`, error)
      throw error
    }
  }

  /**
   * 获取项
   */
  async getItem<T = unknown>(key: string): Promise<T | null> {
    try {
      await this.init()
      const storageKey = getStorageKey(key)
      const encryptedValue = localStorage.getItem(storageKey)

      if (!encryptedValue) {
        return null
      }

      const decryptedValue = await cryptoService.decrypt(encryptedValue)
      return JSON.parse(decryptedValue) as T
    } catch (error) {
      console.error(`读取数据失败 [${key}]:`, error)
      return null
    }
  }

  /**
   * 移除项
   */
  removeItem(key: string): void {
    try {
      const storageKey = getStorageKey(key)
      localStorage.removeItem(storageKey)
    } catch (error) {
      console.error(`删除数据失败 [${key}]:`, error)
    }
  }

  /**
   * 清空所有存储
   */
  clear(): void {
    try {
      // 只清除带有应用前缀的项
      const keys = Object.keys(localStorage)
      const prefix = storageConfig.prefix

      keys.forEach((key) => {
        if (key.startsWith(prefix)) {
          localStorage.removeItem(key)
        }
      })
    } catch (error) {
      console.error('清空存储失败:', error)
    }
  }

  /**
   * 检查项是否存在
   */
  hasItem(key: string): boolean {
    const storageKey = getStorageKey(key)
    return localStorage.getItem(storageKey) !== null
  }

  /**
   * 获取所有 Key
   */
  getAllKeys(): string[] {
    const keys = Object.keys(localStorage)
    const prefix = storageConfig.prefix

    return keys.filter((key) => key.startsWith(prefix)).map((key) => key.slice(prefix.length))
  }
}

/**
 * 导出单例实例
 */
export const secureStorage = new SecureStorage()

/**
 * 便捷的 Token 存储方法
 */
export const tokenStorage = {
  /**
   * 保存 Access Token
   */
  async setAccessToken(token: string): Promise<void> {
    await secureStorage.setItem(storageConfig.tokenKey, token)
  },

  /**
   * 获取 Access Token
   */
  async getAccessToken(): Promise<string | null> {
    return secureStorage.getItem<string>(storageConfig.tokenKey)
  },

  /**
   * 保存 Refresh Token
   */
  async setRefreshToken(token: string): Promise<void> {
    await secureStorage.setItem(storageConfig.refreshTokenKey, token)
  },

  /**
   * 获取 Refresh Token
   */
  async getRefreshToken(): Promise<string | null> {
    return secureStorage.getItem<string>(storageConfig.refreshTokenKey)
  },

  /**
   * 清除所有 Token
   */
  clearTokens(): void {
    secureStorage.removeItem(storageConfig.tokenKey)
    secureStorage.removeItem(storageConfig.refreshTokenKey)
  },
}

/**
 * 便捷的用户存储方法
 */
export const userStorage = {
  /**
   * 保存用户信息
   */
  async setUser(user: unknown): Promise<void> {
    await secureStorage.setItem(storageConfig.userKey, user)
  },

  /**
   * 获取用户信息
   */
  async getUser<T = unknown>(): Promise<T | null> {
    return secureStorage.getItem<T>(storageConfig.userKey)
  },

  /**
   * 清除用户信息
   */
  clearUser(): void {
    secureStorage.removeItem(storageConfig.userKey)
  },
}

export default secureStorage
