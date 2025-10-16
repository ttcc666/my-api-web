import { ref, readonly } from 'vue'
import { DeviceApi, type DeviceInfoDto } from '@/api/device'

export function useSystemMonitor() {
  const deviceInfo = ref<DeviceInfoDto | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  const fetchDeviceInfo = async () => {
    try {
      loading.value = true
      error.value = null
      deviceInfo.value = await DeviceApi.getDeviceInfo()
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
    fetchDeviceInfo,
  }
}
