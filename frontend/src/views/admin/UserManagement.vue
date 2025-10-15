
<template>
  <div class="user-management">
    <div class="user-management__header">
      <h1>用户管理</h1>
    </div>
    <a-table
      :columns="columns"
      :data-source="tableData"
      :loading="loading"
      :pagination="pagination"
      row-key="id"
    >
      <template #bodyCell="{ column, record }">
        <template v-if="column.key === 'isActive'">
          <a-tag :color="record.isActive ? 'green' : 'red'">
            {{ record.isActive ? '激活' : '禁用' }}
          </a-tag>
        </template>
        <template v-else-if="column.key === 'actions'">
          <a-button type="link" @click="handleEdit(record)">编辑权限</a-button>
        </template>
      </template>
    </a-table>

    <a-modal
      v-model:open="showModal"
      title="编辑用户"
      width="800px"
      :confirm-loading="submitLoading"
      @ok="handleSubmit"
    >
      <a-form layout="vertical">
        <a-form-item label="用户名">
          <a-input :value="currentUser.username" disabled />
        </a-form-item>
        <a-form-item label="角色">
          <a-select
            v-model:value="currentUser.roleIds"
            mode="multiple"
            :options="roleOptions"
            placeholder="选择角色"
          />
        </a-form-item>
        <a-form-item label="直接权限">
          <a-transfer
            v-model:targetKeys="currentUser.directPermissionIds"
            :data-source="permissionDataSource"
            :titles="['可用权限', '已分配权限']"
            :render="renderPermissionItem"
            :filter-option="filterPermission"
            show-search
            :list-style="transferListStyle"
          />
        </a-form-item>
      </a-form>
    </a-modal>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import type { ColumnsType } from 'ant-design-vue/es/table'
import {
  useUserManagement,
  type UserWithRolesAndPermissions,
} from '@/composables/useUserManagement'
import { useRoleManagement } from '@/composables/useRoleManagement'
import { usePermissions } from '@/composables/usePermissions'

type TableRecord = UserWithRolesAndPermissions

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
const tableData = computed(() => users.value as TableRecord[])

const showModal = ref(false)
const submitLoading = ref(false)

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
  roles.value.map((role) => ({
    label: role.name,
    value: role.id,
  })),
)

interface TransferOption {
  key: string
  title: string
  disabled?: boolean
}

const permissionDataSource = computed<TransferOption[]>(() =>
  permissions.value.map((permission) => ({
    key: permission.id,
    title: `${permission.group ? `[${permission.group}] ` : ''}${permission.displayName}`,
  })),
)

const columns: ColumnsType<TableRecord> = [
  { title: '用户名', dataIndex: 'username', key: 'username' },
  { title: '邮箱', dataIndex: 'email', key: 'email' },
  { title: '状态', dataIndex: 'isActive', key: 'isActive' },
  { title: '操作', key: 'actions' },
]

const pagination = {
  pageSize: 10,
}

const transferListStyle = {
  width: '45%',
  height: '320px',
}

function filterPermission(inputValue: string, item: TransferOption) {
  return (item.title || '').toLowerCase().includes(inputValue.toLowerCase())
}

function renderPermissionItem(item: TransferOption) {
  return item.title || ''
}

async function handleEdit(user: TableRecord) {
  const userPermissions = await getUserPermissions(user.id)
  if (!userPermissions) {
    return
  }

  currentUser.value = {
    ...user,
    roleIds: userPermissions.roles.map((role) => role.id),
    directPermissionIds: userPermissions.directPermissions.map((permission) => permission.id),
    roles: userPermissions.roles,
    directPermissions: userPermissions.directPermissions,
  }

  showModal.value = true
}

async function handleSubmit() {
  submitLoading.value = true
  const success = await updateUserRolesAndPermissions(
    currentUser.value.id,
    currentUser.value.roleIds,
    currentUser.value.directPermissionIds,
  )
  submitLoading.value = false

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
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.user-management__header h1 {
  margin: 0;
  font-size: 24px;
  font-weight: 600;
}
</style>
