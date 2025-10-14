import { ref } from 'vue'
import { useMessage } from 'naive-ui'
import { RolesApi } from '@/api'
import type { RoleDto, CreateRoleDto, UpdateRoleDto } from '@/types/api'

export function useRoleManagement() {
  const message = useMessage()
  const loading = ref(false)
  const roles = ref<RoleDto[]>([])

  async function fetchRoles() {
    try {
      loading.value = true
      const roleList = await RolesApi.getAllRoles()
      roles.value = Array.isArray(roleList) ? roleList : []
    } catch (error) {
      console.error('获取角色列表失败:', error)
      message.error('获取角色列表失败')
    } finally {
      loading.value = false
    }
  }

  async function createRole(roleData: CreateRoleDto): Promise<boolean> {
    try {
      await RolesApi.createRole(roleData)
      message.success('创建成功')
      await fetchRoles()
      return true
    } catch (error) {
      console.error('创建失败:', error)
      message.error('创建失败')
      return false
    }
  }

  async function updateRole(id: string, roleData: UpdateRoleDto): Promise<boolean> {
    try {
      await RolesApi.updateRole(id, roleData)
      message.success('更新成功')
      await fetchRoles()
      return true
    } catch (error) {
      console.error('更新失败:', error)
      message.error('更新失败')
      return false
    }
  }

  async function deleteRole(id: string): Promise<boolean> {
    try {
      await RolesApi.deleteRole(id)
      message.success('删除成功')
      await fetchRoles()
      return true
    } catch (error) {
      console.error('删除失败:', error)
      message.error('删除失败')
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
