import { ref } from 'vue'
import { UsersApi, PermissionsApi } from '@/api'
import type {
  UserDto,
  AssignUserRolesDto,
  AssignUserPermissionsDto,
  UserPermissionInfoDto,
} from '@/types/api'
import { useFeedback } from '@/composables/useFeedback'

export interface UserWithRolesAndPermissions extends UserDto {
  roleIds: string[]
  directPermissionIds: string[]
  roles: Array<{ id: string; name: string }>
  directPermissions: Array<{ id: string; name: string }>
}

export function useUserManagement() {
  const loading = ref(false)
  const users = ref<UserWithRolesAndPermissions[]>([])
  const { message: feedbackMessage } = useFeedback()

  async function fetchUsers() {
    try {
      loading.value = true
      const data = await UsersApi.getAllUsers()
      const userList = Array.isArray(data) ? data : []
      users.value = userList.map((u: UserDto) => ({
        ...u,
        roleIds: [],
        directPermissionIds: [],
        roles: [],
        directPermissions: [],
      }))
    } catch (error) {
      console.error('获取用户列表失败:', error)
      feedbackMessage.error('获取用户列表失败')
    } finally {
      loading.value = false
    }
  }

  async function getUserPermissions(userId: string): Promise<UserPermissionInfoDto | null> {
    try {
      loading.value = true
      return await PermissionsApi.getUserPermissions(userId)
    } catch (error) {
      console.error('获取用户权限信息失败:', error)
      feedbackMessage.error('获取用户权限信息失败')
      return null
    } finally {
      loading.value = false
    }
  }

  async function updateUserRolesAndPermissions(
    userId: string,
    roleIds: string[],
    permissionIds: string[],
  ): Promise<boolean> {
    try {
      loading.value = true

      const rolesData: AssignUserRolesDto = { roleIds }
      await UsersApi.assignUserRoles(userId, rolesData)

      const permissionsData: AssignUserPermissionsDto = { permissionIds }
      await PermissionsApi.assignUserPermissions(userId, permissionsData)

      feedbackMessage.success('更新成功')
      await fetchUsers()
      return true
    } catch (error) {
      console.error('更新失败:', error)
      feedbackMessage.error('更新失败')
      return false
    } finally {
      loading.value = false
    }
  }

  return {
    loading,
    users,
    fetchUsers,
    getUserPermissions,
    updateUserRolesAndPermissions,
  }
}
