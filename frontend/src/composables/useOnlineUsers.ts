import { ref, onUnmounted } from 'vue'
import { storeToRefs } from 'pinia'
import { useOnlineUserStore } from '@/stores/modules/hub/onlineUser'
import type { OnlineUserQueryParams, OnlineUserStatus } from '@/types/api'

export function useOnlineUsers() {
  const onlineUserStore = useOnlineUserStore()
  const { onlineUsers, statistics, loading, statisticsLoading, error, isConnected, connectionId } =
    storeToRefs(onlineUserStore)

  // 查询参数
  const queryParams = ref<OnlineUserQueryParams>({
    pageNumber: 1,
    pageSize: 20,
  })

  // 分页信息
  const pagination = ref({
    current: 1,
    pageSize: 20,
    total: 0,
    showTotal: (total: number) => `共 ${total} 条记录`,
    showSizeChanger: true,
    pageSizeOptions: ['10', '20', '50', '100'],
  })

  // 自动刷新控制
  const autoRefreshEnabled = ref(false)
  const refreshInterval = ref(10000) // 默认10秒
  let refreshTimer: number | null = null

  // 格式化在线时长
  const formatDuration = (seconds: number): string => {
    const hours = Math.floor(seconds / 3600)
    const minutes = Math.floor((seconds % 3600) / 60)
    const secs = seconds % 60

    if (hours > 0) {
      return `${hours}小时${minutes}分钟`
    } else if (minutes > 0) {
      return `${minutes}分钟${secs}秒`
    } else {
      return `${secs}秒`
    }
  }

  // 获取状态颜色
  const getStatusColor = (status: OnlineUserStatus): string => {
    switch (status) {
      case 'Online':
        return 'green'
      case 'Idle':
        return 'orange'
      case 'Offline':
        return 'red'
      default:
        return 'default'
    }
  }

  // 获取状态文本
  const getStatusText = (status: OnlineUserStatus): string => {
    switch (status) {
      case 'Online':
        return '在线'
      case 'Idle':
        return '空闲'
      case 'Offline':
        return '离线'
      default:
        return '未知'
    }
  }

  // 加载在线用户列表
  const loadOnlineUsers = async (): Promise<void> => {
    try {
      const response = await onlineUserStore.fetchOnlineUsers(queryParams.value)

      // 更新分页信息
      pagination.value.current = response.pageNumber
      pagination.value.pageSize = response.pageSize
      pagination.value.total = response.totalCount
    } catch (err) {
      console.error('[useOnlineUsers] 加载在线用户列表失败', err)
    }
  }

  // 加载统计信息
  const loadStatistics = async (): Promise<void> => {
    try {
      await onlineUserStore.fetchStatistics()
    } catch (err) {
      console.error('[useOnlineUsers] 加载统计信息失败', err)
    }
  }

  // 刷新所有数据
  const refreshAll = async (): Promise<void> => {
    await Promise.all([loadOnlineUsers(), loadStatistics()])
  }

  // 处理分页变化
  const handlePageChange = (page: number, pageSize: number): void => {
    queryParams.value.pageNumber = page
    queryParams.value.pageSize = pageSize
    loadOnlineUsers()
  }

  // 处理筛选变化
  const handleFilterChange = (filters: {
    status?: OnlineUserStatus
    userId?: string
    room?: string
  }): void => {
    queryParams.value = {
      ...queryParams.value,
      ...filters,
      pageNumber: 1, // 重置到第一页
    }
    loadOnlineUsers()
  }

  // 强制下线
  const handleForceDisconnect = async (connectionId: string, reason?: string): Promise<void> => {
    await onlineUserStore.forceDisconnect(connectionId, reason)
  }

  // 手动清理超时连接
  const handleCleanup = async (timeoutMinutes?: number): Promise<number> => {
    return await onlineUserStore.cleanup(timeoutMinutes)
  }

  // 启动自动刷新
  const startAutoRefresh = (): void => {
    if (refreshTimer) {
      return
    }

    autoRefreshEnabled.value = true
    refreshTimer = window.setInterval(() => {
      refreshAll()
    }, refreshInterval.value)
  }

  // 停止自动刷新
  const stopAutoRefresh = (): void => {
    if (refreshTimer) {
      clearInterval(refreshTimer)
      refreshTimer = null
    }
    autoRefreshEnabled.value = false
  }

  // 设置刷新间隔
  const setRefreshInterval = (interval: number): void => {
    refreshInterval.value = interval

    // 如果正在自动刷新,重新启动
    if (autoRefreshEnabled.value) {
      stopAutoRefresh()
      startAutoRefresh()
    }
  }

  // 组件卸载时清理
  onUnmounted(() => {
    stopAutoRefresh()
  })

  return {
    // 状态
    onlineUsers,
    statistics,
    loading,
    statisticsLoading,
    error,
    isConnected,
    connectionId,
    queryParams,
    pagination,
    autoRefreshEnabled,
    refreshInterval,

    // 方法
    loadOnlineUsers,
    loadStatistics,
    refreshAll,
    handlePageChange,
    handleFilterChange,
    handleForceDisconnect,
    handleCleanup,
    startAutoRefresh,
    stopAutoRefresh,
    setRefreshInterval,

    // 工具方法
    formatDuration,
    getStatusColor,
    getStatusText,
  }
}
