import { ref, computed, readonly } from 'vue'
import { defineStore } from 'pinia'
import type { UserPermissionInfoDto } from '@/types/api'
import { AuthApi } from '@/api'

type UserPermissionInfo = UserPermissionInfoDto

export const usePermissionStore = defineStore('permission', () => {
  const userPermissions = ref<UserPermissionInfo | null>(null)
  const permissions = ref<string[]>([])
  const roles = ref<string[]>([])
  const permissionsLoaded = ref(false)
  const loading = ref(false)
  const error = ref<string | null>(null)

  const hasPermission = computed(() => (permission: string) => {
    return permissions.value.includes(permission)
  })

  const hasRole = computed(() => (role: string) => {
    return roles.value.includes(role)
  })

  const hasAnyPermission = computed(() => (permissionList: string[]) => {
    return permissionList.some((permission) => permissions.value.includes(permission))
  })

  const hasAllPermissions = computed(() => (permissionList: string[]) => {
    return permissionList.every((permission) => permissions.value.includes(permission))
  })

  const setUserPermissions = (permissionInfo: UserPermissionInfo) => {
    if (!permissionInfo) return
    userPermissions.value = permissionInfo
    permissions.value = (permissionInfo.effectivePermissions || []).map((p) => p.name)
    roles.value = (permissionInfo.roles || []).map((r) => r.name)
  }

  const clearUserPermissions = () => {
    userPermissions.value = null
    permissions.value = []
    roles.value = []
    permissionsLoaded.value = false
  }

  const loadUserPermissions = async () => {
    if (permissionsLoaded.value) return

    try {
      loading.value = true
      const permissionInfo = await AuthApi.getCurrentUserPermissions()
      if (permissionInfo) {
        setUserPermissions(permissionInfo)
        permissionsLoaded.value = true
      }
    } catch (err) {
      console.error('加载用户权限失败:', err)
      error.value = '加载用户权限失败'
      permissionsLoaded.value = false
      throw err
    } finally {
      loading.value = false
    }
  }

  return {
    userPermissions: readonly(userPermissions),
    permissions: readonly(permissions),
    roles: readonly(roles),
    permissionsLoaded: readonly(permissionsLoaded),
    loading: readonly(loading),
    error: readonly(error),
    hasPermission,
    hasRole,
    hasAnyPermission,
    hasAllPermissions,
    setUserPermissions,
    clearUserPermissions,
    loadUserPermissions,
  }
})
