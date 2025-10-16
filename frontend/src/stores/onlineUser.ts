import { ref, computed } from 'vue'
import { defineStore } from 'pinia'
import type {
  OnlineUserDto,
  OnlineUserStatisticsDto,
  OnlineUserListResponseDto,
  OnlineUserQueryParams,
} from '@/types/api'
import { OnlineUsersApi } from '@/api/onlineUsers'
import { createSignalRService, destroySignalRService } from '@/services/signalr'
import type { SignalRService } from '@/services/signalr'
import { message, Modal } from 'ant-design-vue'
import router from '@/router'

export const useOnlineUserStore = defineStore('onlineUser', () => {
  // SignalR 连接实例
  const signalRService = ref<SignalRService | null>(null)

  // 在线用户列表
  const onlineUsers = ref<OnlineUserDto[]>([])

  // 统计信息
  const statistics = ref<OnlineUserStatisticsDto | null>(null)

  // 加载状态
  const loading = ref(false)
  const statisticsLoading = ref(false)

  // 错误信息
  const error = ref<string | null>(null)

  // 连接状态
  const isConnected = computed(() => signalRService.value?.isConnected() ?? false)

  // 连接ID
  const connectionId = computed(() => signalRService.value?.getConnectionId() ?? null)

  /**
   * 初始化 SignalR 连接
   */
  const initConnection = async (tokenFactory: () => string): Promise<void> => {
    try {
      // 如果已经连接,先断开
      if (signalRService.value) {
        await disconnect()
      }

      // 创建新的 SignalR 服务实例
      signalRService.value = createSignalRService(tokenFactory)

      // 注册强制下线事件监听器
      signalRService.value.onForceDisconnect((data) => {
        console.log('[OnlineUser] 收到强制下线通知', data)

        // 显示被踢下线的 Modal 对话框
        Modal.warning({
          title: '账号已被强制下线',
          content: data.reason || '您的账号已被管理员强制下线，请重新登录。',
          okText: '确定',
          onOk: async () => {
            // 执行登出操作
            await handleForceLogout()
          },
        })

        // 设置 3 秒后自动执行登出
        setTimeout(async () => {
          await handleForceLogout()
        }, 3000)
      })

      // 建立连接
      await signalRService.value.connect()

      console.log('[OnlineUser] SignalR 连接已建立')
    } catch (err) {
      error.value = (err as Error).message || '建立 SignalR 连接失败'
      console.error('[OnlineUser] 连接失败', err)
      throw err
    }
  }

  /**
   * 处理强制登出
   */
  const handleForceLogout = async (): Promise<void> => {
    try {
      // 导入 authStore (避免循环依赖,在函数内部导入)
      const { useAuthStore } = await import('./auth')
      const authStore = useAuthStore()

      // 断开 SignalR 连接
      await disconnect()

      // 执行登出
      authStore.logout()

      // 跳转到登录页,并传递提示信息
      await router.push({
        path: '/login',
        query: { reason: 'force_logout', message: '您已被管理员强制下线' },
      })
    } catch (err) {
      console.error('[OnlineUser] 强制登出失败', err)
    }
  }

  /**
   * 断开 SignalR 连接
   */
  const disconnect = async (): Promise<void> => {
    try {
      if (signalRService.value) {
        await signalRService.value.disconnect()
        destroySignalRService()
        signalRService.value = null
      }
      console.log('[OnlineUser] SignalR 连接已断开')
    } catch (err) {
      console.error('[OnlineUser] 断开连接失败', err)
    }
  }

  /**
   * 获取在线用户列表
   */
  const fetchOnlineUsers = async (
    params?: OnlineUserQueryParams,
  ): Promise<OnlineUserListResponseDto> => {
    try {
      loading.value = true
      error.value = null

      const response = await OnlineUsersApi.getOnlineUsers(params)
      onlineUsers.value = response.users

      return response
    } catch (err) {
      error.value = (err as Error).message || '获取在线用户列表失败'
      throw err
    } finally {
      loading.value = false
    }
  }

  /**
   * 获取统计信息
   */
  const fetchStatistics = async (): Promise<OnlineUserStatisticsDto> => {
    try {
      statisticsLoading.value = true
      error.value = null

      const data = await OnlineUsersApi.getStatistics()
      statistics.value = data

      return data
    } catch (err) {
      error.value = (err as Error).message || '获取统计信息失败'
      throw err
    } finally {
      statisticsLoading.value = false
    }
  }

  /**
   * 获取在线用户数量
   */
  const fetchCount = async (): Promise<number> => {
    try {
      const data = await OnlineUsersApi.getCount()
      return data.count
    } catch (err) {
      console.error('[OnlineUser] 获取在线用户数量失败', err)
      return 0
    }
  }

  /**
   * 强制下线指定连接
   */
  const forceDisconnect = async (connectionId: string, reason?: string): Promise<void> => {
    try {
      await OnlineUsersApi.forceDisconnect(connectionId, reason)
      message.success('强制下线成功')

      // 刷新列表
      await fetchOnlineUsers()
    } catch (err) {
      error.value = (err as Error).message || '强制下线失败'
      message.error(error.value)
      throw err
    }
  }

  /**
   * 手动清理超时连接
   */
  const cleanup = async (timeoutMinutes?: number): Promise<number> => {
    try {
      const result = await OnlineUsersApi.cleanup(timeoutMinutes)
      message.success(`清理完成,共清理 ${result.cleanedCount} 个超时连接`)

      // 刷新列表
      await fetchOnlineUsers()

      return result.cleanedCount
    } catch (err) {
      error.value = (err as Error).message || '清理超时连接失败'
      message.error(error.value)
      throw err
    }
  }

  /**
   * 清空状态
   */
  const reset = (): void => {
    onlineUsers.value = []
    statistics.value = null
    error.value = null
  }

  return {
    // 状态
    signalRService,
    onlineUsers,
    statistics,
    loading,
    statisticsLoading,
    error,
    isConnected,
    connectionId,

    // 方法
    initConnection,
    disconnect,
    fetchOnlineUsers,
    fetchStatistics,
    fetchCount,
    forceDisconnect,
    cleanup,
    reset,
  }
})
