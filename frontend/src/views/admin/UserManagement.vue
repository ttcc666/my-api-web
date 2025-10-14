<template>
  <div class="user-management">
    <h1>用户管理</h1>
    <n-data-table :columns="columns" :data="users" :loading="loading" :pagination="pagination" />

    <!-- 编辑用户角色和权限的模态框 -->
    <n-modal v-model:show="showModal" preset="card" style="width: 800px" title="编辑用户">
      <n-form :model="currentUser">
        <n-form-item label="用户名">
          <n-input :value="currentUser.username" disabled />
        </n-form-item>
        <n-form-item label="角色">
          <n-select
            v-model:value="currentUser.roleIds"
            multiple
            :options="roleOptions"
            placeholder="选择角色"
          />
        </n-form-item>
        <n-form-item label="直接权限">
          <n-transfer
            v-model:value="currentUser.directPermissionIds"
            :options="permissionOptions"
            source-filterable
            target-filterable
            source-title="可用权限"
            target-title="已分配权限"
          />
        </n-form-item>
      </n-form>
      <template #footer>
        <n-button @click="showModal = false">取消</n-button>
        <n-button type="primary" @click="handleSubmit">保存</n-button>
      </template>
    </n-modal>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, h } from 'vue'
import {
  NButton,
  NDataTable,
  NModal,
  NForm,
  NFormItem,
  NInput,
  NSelect,
  NTransfer,
  useMessage,
  type DataTableColumns,
} from 'naive-ui'
import UserApi from '@/api/user'
import RoleApi from '@/api/role'
import { UsersApi } from '@/api/users'
import { PermissionsApi } from '@/api/permissions'
import type {
  UserDto,
  RoleDto,
  PermissionDto,
  AssignUserRolesDto,
  AssignUserPermissionsDto,
} from '@/types/api'

// 类型别名以保持兼容性
type User = UserDto
type Role = RoleDto
type Permission = PermissionDto

interface UserWithRolesAndPermissions extends User {
  roleIds: string[]
  directPermissionIds: string[]
  roles: Role[]
  directPermissions: Permission[]
}

const message = useMessage()
const loading = ref(false)
const users = ref<UserWithRolesAndPermissions[]>([])
const roles = ref<Role[]>([])
const permissions = ref<Permission[]>([])
const showModal = ref(false)

const defaultUser = (): UserWithRolesAndPermissions => ({
  id: '',
  username: '',
  email: '',
  isActive: false,
  createdTime: '',
  roleIds: [],
  directPermissionIds: [],
  roles: [],
  directPermissions: [],
})

const currentUser = ref<UserWithRolesAndPermissions>(defaultUser())

const roleOptions = computed(() =>
  roles.value.map((r) => ({
    label: r.name,
    value: r.id,
  })),
)

const permissionOptions = computed(() =>
  permissions.value.map((p) => ({
    label: `${p.group ? `[${p.group}] ` : ''}${p.displayName}`,
    value: p.id,
  })),
)

const columns: DataTableColumns<User> = [
  { title: '用户名', key: 'username' },
  { title: '邮箱', key: 'email' },
  { title: '状态', key: 'isActive', render: (row) => (row.isActive ? '激活' : '禁用') },
  {
    title: '操作',
    key: 'actions',
    render(row) {
      return h(
        NButton,
        {
          size: 'small',
          type: 'primary',
          onClick: () => handleEdit(row as UserWithRolesAndPermissions),
          'v-permission': "'manage:user_permissions'",
        },
        { default: () => '编辑权限' },
      )
    },
  },
]

const pagination = {
  pageSize: 10,
}

async function fetchUsers() {
  try {
    loading.value = true
    const data = await UserApi.getAllUsers()
    console.log('Fetched users:', data) // 添加日志
    // API 现在直接返回用户数组
    const userList = Array.isArray(data) ? data : []
    users.value = userList.map((u: User) => ({
      ...u,
      roleIds: [],
      directPermissionIds: [],
      roles: [],
      directPermissions: [],
    }))
  } catch (error) {
    console.error('获取用户列表失败:', error)
    message.error('获取用户列表失败')
  } finally {
    loading.value = false
  }
}

async function fetchRolesAndPermissions() {
  try {
    ;[roles.value, permissions.value] = await Promise.all([
      RoleApi.getAllRoles(),
      RoleApi.getAllPermissions(),
    ])
  } catch (error) {
    console.error('获取角色和权限列表失败:', error)
    message.error('获取角色和权限列表失败')
  }
}

async function handleEdit(user: UserWithRolesAndPermissions) {
  try {
    loading.value = true
    // 从后端获取用户的完整权限信息
    const userPermissions = await PermissionsApi.getUserPermissions(user.id)

    currentUser.value = {
      ...user,
      roleIds: userPermissions.roles.map((r) => r.id),
      directPermissionIds: userPermissions.directPermissions.map((p) => p.id),
      roles: userPermissions.roles,
      directPermissions: userPermissions.directPermissions,
    }
    showModal.value = true
  } catch (error) {
    console.error('获取用户权限信息失败:', error)
    message.error('获取用户权限信息失败')
  } finally {
    loading.value = false
  }
}

async function handleSubmit() {
  try {
    loading.value = true

    // 更新用户角色
    const rolesData: AssignUserRolesDto = {
      roleIds: currentUser.value.roleIds,
    }
    await UsersApi.assignUserRoles(currentUser.value.id, rolesData)

    // 更新用户直接权限
    const permissionsData: AssignUserPermissionsDto = {
      permissionIds: currentUser.value.directPermissionIds,
    }
    await PermissionsApi.assignUserPermissions(currentUser.value.id, permissionsData)

    message.success('更新成功')
    showModal.value = false
    await fetchUsers() // 重新加载用户数据
  } catch (error) {
    console.error('更新失败:', error)
    message.error('更新失败')
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchUsers()
  fetchRolesAndPermissions()
})
</script>

<style scoped>
.user-management {
  padding: 20px;
}
</style>
