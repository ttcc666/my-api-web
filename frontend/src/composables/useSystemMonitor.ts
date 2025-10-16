import { ref, readonly } from 'vue'
import { DeviceApi } from '@/api'
import type { DeviceInfoDto } from '@/api/modules/system/device'

// 历史数据点接口
interface HistoryDataPoint {
  timestamp: number
  cpuUsage: number
  memoryUsage: number
}

export function useSystemMonitor() {
  const deviceInfo = ref<DeviceInfoDto | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  // 新增状态: 历史数据和自动刷新
  const historyData = ref<HistoryDataPoint[]>([])
  const autoRefresh = ref(false)
  const refreshInterval = ref(10000) // 默认10秒
  let intervalId: number | null = null

  // 添加历史数据点
  const addHistoryPoint = () => {
    if (!deviceInfo.value) return

    const usedMemory = deviceInfo.value.totalMemoryMB - deviceInfo.value.availableMemoryMB
    const memoryUsagePercent = (usedMemory / deviceInfo.value.totalMemoryMB) * 100

    historyData.value.push({
      timestamp: Date.now(),
      cpuUsage: deviceInfo.value.cpuUsage,
      memoryUsage: memoryUsagePercent,
    })

    // 保持最近30个数据点
    if (historyData.value.length > 30) {
      historyData.value.shift()
    }
  }

  // 启动自动刷新
  const startAutoRefresh = () => {
    if (intervalId !== null) return // 防止重复启动

    autoRefresh.value = true
    intervalId = window.setInterval(() => {
      fetchDeviceInfo()
    }, refreshInterval.value)
  }

  // 停止自动刷新
  const stopAutoRefresh = () => {
    if (intervalId !== null) {
      clearInterval(intervalId)
      intervalId = null
    }
    autoRefresh.value = false
  }

  // 设置刷新间隔
  const setRefreshInterval = (interval: number) => {
    refreshInterval.value = interval

    // 如果正在自动刷新,重新启动以应用新间隔
    if (autoRefresh.value) {
      stopAutoRefresh()
      startAutoRefresh()
    }
  }

  const fetchDeviceInfo = async () => {
    try {
      loading.value = true
      error.value = null
      deviceInfo.value = await DeviceApi.getDeviceInfo()

      // 获取数据后添加历史点
      addHistoryPoint()
    } catch (err) {
      error.value = (err instanceof Error ? err.message : String(err)) || '获取设备信息失败'
      throw err
    } finally {
      loading.value = false
    }
  }

  return {
    deviceInfo: readonly(deviceInfo),
    loading: readonly(loading),
    error: readonly(error),
    historyData: readonly(historyData),
    autoRefresh: readonly(autoRefresh),
    refreshInterval: readonly(refreshInterval),
    fetchDeviceInfo,
    startAutoRefresh,
    stopAutoRefresh,
    setRefreshInterval,
  }
}
