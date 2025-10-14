import { ref } from 'vue'
import { useMessage } from 'naive-ui'
import { PermissionsApi } from '@/api'
import type { PermissionDto } from '@/types/api'

export function usePermissions() {
  const message = useMessage()
  const permissions = ref<PermissionDto[]>([])

  async function fetchPermissions() {
    try {
      const permissionList = await PermissionsApi.getAllPermissions()
      permissions.value = Array.isArray(permissionList) ? permissionList : []
    } catch (error) {
      console.error('获取权限列表失败:', error)
      message.error('获取权限列表失败')
    }
  }

  return {
    permissions,
    fetchPermissions,
  }
}
