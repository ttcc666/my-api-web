import { ref, computed } from 'vue'
import { defineStore } from 'pinia'
import type { UserPermissionInfoDto } from '@/types/api'
import { AuthApi } from '@/api'
import { cacheConfig } from '@/config'

type UserPermissionInfo = UserPermissionInfoDto

const PERMISSIONS_TTL = cacheConfig.permissionCacheExpiry

export const usePermissionStore = defineStore(
  'permission',
  () => {
    const userPermissions = ref<UserPermissionInfo | null>(null)
    const permissions = ref<string[]>([])
    const roles = ref<string[]>([])
    const permissionsLoaded = ref(false)
    const loading = ref(false)
    const error = ref<string | null>(null)
    const lastLoadedAt = ref<number | null>(null)

    let loadPromise: Promise<void> | null = null

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

    const isCacheValid = () => {
      if (!permissionsLoaded.value || !lastLoadedAt.value) {
        return false
      }
      return Date.now() - lastLoadedAt.value < PERMISSIONS_TTL
    }

    const setUserPermissions = (permissionInfo: UserPermissionInfo) => {
      if (!permissionInfo) return
      userPermissions.value = permissionInfo
      permissions.value = (permissionInfo.effectivePermissions || []).map((p) => p.name)
      roles.value = (permissionInfo.roles || []).map((r) => r.name)
      permissionsLoaded.value = true
      lastLoadedAt.value = Date.now()
      error.value = null
    }

    const clearUserPermissions = () => {
      userPermissions.value = null
      permissions.value = []
      roles.value = []
      permissionsLoaded.value = false
      lastLoadedAt.value = null
      error.value = null
    }

    const loadUserPermissions = async (forceRefresh = false) => {
      if (!forceRefresh && isCacheValid()) {
        return
      }

      if (loadPromise && !forceRefresh) {
        return loadPromise
      }

      if (forceRefresh) {
        clearUserPermissions()
        loadPromise = null
      }

      loadPromise = (async () => {
        try {
          loading.value = true
          error.value = null
          const permissionInfo = await AuthApi.getCurrentUserPermissions()
          if (permissionInfo) {
            setUserPermissions(permissionInfo)
          } else {
            clearUserPermissions()
          }
        } catch (err) {
          console.error('加载用户权限失败:', err)
          error.value = '加载用户权限失败'
          permissionsLoaded.value = false
          lastLoadedAt.value = null
          throw err
        } finally {
          loading.value = false
          loadPromise = null
        }
      })()

      return loadPromise
    }

    return {
      userPermissions,
      permissions,
      roles,
      permissionsLoaded,
      loading,
      error,
      lastLoadedAt,
      hasPermission,
      hasRole,
      hasAnyPermission,
      hasAllPermissions,
      setUserPermissions,
      clearUserPermissions,
      loadUserPermissions,
    }
  },
  {
    persist: {
      pick: ['userPermissions', 'permissions', 'roles', 'permissionsLoaded', 'lastLoadedAt'],
    },
  },
)
