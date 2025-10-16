import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr'
import { apiConfig } from '@/config'

/**
 * SignalR 服务类
 * 负责管理 SignalR Hub 连接、心跳保活和事件监听
 */
export class SignalRService {
  private connection: HubConnection | null = null
  private heartbeatTimer: number | null = null
  private readonly heartbeatInterval = 2 * 60 * 1000 // 2分钟
  private readonly hubUrl: string
  private accessTokenFactory: () => string

  constructor(hubPath: string = '/hubs/chat', tokenFactory: () => string) {
    // 构建完整的 Hub URL
    const baseUrl = apiConfig.baseURL.replace('/api', '') // 移除 /api 后缀
    this.hubUrl = `${baseUrl}${hubPath}`
    this.accessTokenFactory = tokenFactory
  }

  /**
   * 建立 SignalR 连接
   */
  async connect(): Promise<void> {
    if (this.connection && this.connection.state === HubConnectionState.Connected) {
      console.log('[SignalR] 连接已存在,无需重复连接')
      return
    }

    try {
      // 创建连接
      this.connection = new HubConnectionBuilder()
        .withUrl(this.hubUrl, {
          accessTokenFactory: this.accessTokenFactory,
        })
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: (retryContext) => {
            // 自定义重连延迟策略
            if (retryContext.previousRetryCount < 3) {
              return 2000 // 前3次重试延迟2秒
            } else if (retryContext.previousRetryCount < 6) {
              return 5000 // 第4-6次重试延迟5秒
            } else {
              return 10000 // 之后延迟10秒
            }
          },
        })
        .configureLogging(LogLevel.Information)
        .build()

      // 监听连接关闭事件
      this.connection.onclose((error) => {
        console.log('[SignalR] 连接已关闭', error)
        this.stopHeartbeat()
      })

      // 监听重连中事件
      this.connection.onreconnecting((error) => {
        console.warn('[SignalR] 正在重连...', error)
        this.stopHeartbeat()
      })

      // 监听重连成功事件
      this.connection.onreconnected((connectionId) => {
        console.log('[SignalR] 重连成功', connectionId)
        this.startHeartbeat()
      })

      // 启动连接
      await this.connection.start()
      console.log('[SignalR] 连接成功', this.connection.connectionId)

      // 启动心跳
      this.startHeartbeat()
    } catch (error) {
      console.error('[SignalR] 连接失败', error)
      throw error
    }
  }

  /**
   * 断开 SignalR 连接
   */
  async disconnect(): Promise<void> {
    this.stopHeartbeat()

    if (this.connection) {
      try {
        await this.connection.stop()
        console.log('[SignalR] 连接已断开')
      } catch (error) {
        console.error('[SignalR] 断开连接失败', error)
      } finally {
        this.connection = null
      }
    }
  }

  /**
   * 启动心跳定时器
   */
  private startHeartbeat(): void {
    if (this.heartbeatTimer) {
      return
    }

    console.log('[SignalR] 启动心跳定时器')

    // 立即发送一次心跳
    this.sendHeartbeat()

    // 设置定时器
    this.heartbeatTimer = window.setInterval(() => {
      this.sendHeartbeat()
    }, this.heartbeatInterval)
  }

  /**
   * 停止心跳定时器
   */
  private stopHeartbeat(): void {
    if (this.heartbeatTimer) {
      clearInterval(this.heartbeatTimer)
      this.heartbeatTimer = null
      console.log('[SignalR] 心跳定时器已停止')
    }
  }

  /**
   * 发送心跳
   */
  private async sendHeartbeat(): Promise<void> {
    if (!this.connection || this.connection.state !== HubConnectionState.Connected) {
      console.warn('[SignalR] 连接未建立,跳过心跳')
      return
    }

    try {
      const response = await this.connection.invoke('Heartbeat')
      console.log('[SignalR] 心跳发送成功', response)
    } catch (error) {
      console.error('[SignalR] 心跳发送失败', error)
    }
  }

  /**
   * 监听服务器推送的强制下线事件
   */
  onForceDisconnect(callback: (data: { reason: string; timestamp: string }) => void): void {
    if (!this.connection) {
      console.warn('[SignalR] 连接未建立,无法监听事件')
      return
    }

    this.connection.on('forceDisconnect', callback)
    console.log('[SignalR] 已注册 forceDisconnect 事件监听器')
  }

  /**
   * 移除强制下线事件监听
   */
  offForceDisconnect(callback?: (data: { reason: string; timestamp: string }) => void): void {
    if (!this.connection) {
      return
    }

    if (callback) {
      this.connection.off('forceDisconnect', callback)
    } else {
      this.connection.off('forceDisconnect')
    }
  }

  /**
   * 获取连接状态
   */
  getState(): HubConnectionState {
    return this.connection?.state ?? HubConnectionState.Disconnected
  }

  /**
   * 获取连接ID
   */
  getConnectionId(): string | null {
    return this.connection?.connectionId ?? null
  }

  /**
   * 检查是否已连接
   */
  isConnected(): boolean {
    return this.connection?.state === HubConnectionState.Connected
  }
}

/**
 * 导出单例实例工厂
 */
let signalRServiceInstance: SignalRService | null = null

export function createSignalRService(tokenFactory: () => string): SignalRService {
  if (!signalRServiceInstance) {
    signalRServiceInstance = new SignalRService('/hubs/chat', tokenFactory)
  }
  return signalRServiceInstance
}

export function destroySignalRService(): void {
  if (signalRServiceInstance) {
    signalRServiceInstance.disconnect()
    signalRServiceInstance = null
  }
}

export default SignalRService
