import { ref } from 'vue'
import { RolesApi } from '@/api'
import type { RoleDto, CreateRoleDto, UpdateRoleDto } from '@/types/api'
import { useFeedback } from '@/composables/useFeedback'

export function useRoleManagement() {
  const loading = ref(false)
  const roles = ref<RoleDto[]>([])
  const { message: feedbackMessage } = useFeedback()

  async function fetchRoles() {
    try {
      loading.value = true
      const roleList = await RolesApi.getAllRoles()
      roles.value = Array.isArray(roleList) ? roleList : []
    } catch (error) {
      console.error('获取角色列表失败:', error)
      feedbackMessage.error('获取角色列表失败')
    } finally {
      loading.value = false
    }
  }

  async function createRole(roleData: CreateRoleDto): Promise<boolean> {
    try {
      await RolesApi.createRole(roleData)
      feedbackMessage.success('创建成功')
      await fetchRoles()
      return true
    } catch (error) {
      console.error('创建失败:', error)
      feedbackMessage.error('创建失败')
      return false
    }
  }

  async function updateRole(id: string, roleData: UpdateRoleDto): Promise<boolean> {
    try {
      await RolesApi.updateRole(id, roleData)
      feedbackMessage.success('更新成功')
      await fetchRoles()
      return true
    } catch (error) {
      console.error('更新失败:', error)
      feedbackMessage.error('更新失败')
      return false
    }
  }

  async function deleteRole(id: string): Promise<boolean> {
    try {
      await RolesApi.deleteRole(id)
      feedbackMessage.success('删除成功')
      await fetchRoles()
      return true
    } catch (error) {
      console.error('删除失败:', error)
      feedbackMessage.error('删除失败')
      return false
    }
  }

  return {
    loading,
    roles,
    fetchRoles,
    createRole,
    updateRole,
    deleteRole,
  }
}
