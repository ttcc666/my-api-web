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
  type DataTableColumns,
} from 'naive-ui'
import {
  useUserManagement,
  type UserWithRolesAndPermissions,
} from '@/composables/useUserManagement'
import { useRoleManagement } from '@/composables/useRoleManagement'
import { usePermissions } from '@/composables/usePermissions'
import type { UserDto } from '@/types/api'

type User = UserDto

const {
  loading: userLoading,
  users,
  fetchUsers,
  getUserPermissions,
  updateUserRolesAndPermissions,
} = useUserManagement()
const { roles, fetchRoles } = useRoleManagement()
const { permissions, fetchPermissions } = usePermissions()

const loading = computed(() => userLoading.value)
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
        },
        { default: () => '编辑权限' },
      )
    },
  },
]

const pagination = {
  pageSize: 10,
}

async function handleEdit(user: UserWithRolesAndPermissions) {
  const userPermissions = await getUserPermissions(user.id)
  if (userPermissions) {
    currentUser.value = {
      ...user,
      roleIds: userPermissions.roles.map((r) => r.id),
      directPermissionIds: userPermissions.directPermissions.map((p) => p.id),
      roles: userPermissions.roles,
      directPermissions: userPermissions.directPermissions,
    }
    showModal.value = true
  }
}

async function handleSubmit() {
  const success = await updateUserRolesAndPermissions(
    currentUser.value.id,
    currentUser.value.roleIds,
    currentUser.value.directPermissionIds,
  )
  if (success) {
    showModal.value = false
  }
}

onMounted(() => {
  fetchUsers()
  fetchRoles()
  fetchPermissions()
})
</script>

<style scoped>
.user-management {
  padding: 20px;
}
</style>
