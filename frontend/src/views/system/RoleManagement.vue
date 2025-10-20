<template>
  <div class="role-management">
    <div class="role-management__header">
      <h1>角色管理</h1>
      <a-button type="primary" @click="handleCreate">创建角色</a-button>
    </div>
    <a-table
      :columns="columns"
      :data-source="tableData"
      :loading="loading"
      :pagination="pagination"
      row-key="id"
    >
      <template #bodyCell="{ column, record }">
        <template v-if="column.key === 'isSystem'">
          <a-tag :color="record.isSystem ? 'blue' : 'default'">
            {{ record.isSystem ? '是' : '否' }}
          </a-tag>
        </template>
        <template v-else-if="column.key === 'isEnabled'">
          <a-tag :color="record.isEnabled ? 'green' : 'red'">
            {{ record.isEnabled ? '启用' : '禁用' }}
          </a-tag>
        </template>
        <template v-else-if="column.key === 'actions'">
          <a-space>
            <a-button type="link" @click="handleEdit(record)">编辑</a-button>
            <a-button type="link" danger :disabled="record.isSystem" @click="handleDelete(record)">
              删除
            </a-button>
          </a-space>
        </template>
      </template>
    </a-table>

    <a-modal
      v-model:open="showModal"
      :title="modalTitle"
      width="600px"
      :confirm-loading="submitLoading"
      @ok="handleSubmit"
    >
      <a-form ref="formRef" layout="vertical" :model="currentRole" :rules="rules">
        <a-form-item name="name" label="角色名称">
          <a-input v-model:value="currentRole.name" :disabled="currentRole.isSystem" />
        </a-form-item>
        <a-form-item name="description" label="描述">
          <a-textarea v-model:value="currentRole.description" :rows="3" />
        </a-form-item>
        <a-form-item label="权限">
          <a-transfer
            v-model:targetKeys="currentRole.permissionIds"
            :data-source="permissionDataSource"
            :titles="['可用权限', '已分配权限']"
            :render="renderPermissionItem"
            :filter-option="filterPermission"
            show-search
            :list-style="transferListStyle"
          />
        </a-form-item>
        <a-form-item v-if="isEdit" label="状态">
          <a-switch
            v-model:checked="currentRole.isEnabled"
            :disabled="currentRole.isSystem"
            checked-children="启用"
            un-checked-children="禁用"
          />
        </a-form-item>
      </a-form>
    </a-modal>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import type { ColumnsType } from 'ant-design-vue/es/table'
import type { FormInstance } from 'ant-design-vue'
import { RolesApi, PermissionsApi } from '@/api'
import { useFeedback } from '@/composables/useFeedback'
import type { RoleDto, PermissionDto, CreateRoleDto, UpdateRoleDto } from '@/types/api'

type Role = RoleDto
type Permission = PermissionDto

const loading = ref(false)
const roles = ref<Role[]>([])
const permissions = ref<Permission[]>([])
const showModal = ref(false)
const isEdit = ref(false)
const submitLoading = ref(false)
const formRef = ref<FormInstance>()
const { message: feedbackMessage, confirm } = useFeedback()

const defaultRole = (): Role & { permissionIds: string[] } => ({
  id: '',
  name: '',
  description: '',
  isSystem: false,
  isEnabled: true,
  creationTime: '',
  permissions: [],
  permissionIds: [],
})

const currentRole = ref(defaultRole())

const modalTitle = computed(() => (isEdit.value ? '编辑角色' : '创建角色'))

interface TransferOption {
  key: string
  title: string
  disabled?: boolean
}

const permissionDataSource = computed<TransferOption[]>(() =>
  permissions.value.map((permission) => ({
    key: permission.id,
    title: `${permission.group ? `[${permission.group}] ` : ''}${permission.displayName}`,
    disabled: false,
  })),
)

const columns: ColumnsType<Role> = [
  { title: '角色名称', dataIndex: 'name', key: 'name' },
  { title: '描述', dataIndex: 'description', key: 'description' },
  { title: '系统角色', dataIndex: 'isSystem', key: 'isSystem' },
  { title: '状态', dataIndex: 'isEnabled', key: 'isEnabled' },
  { title: '操作', key: 'actions' },
]

const pagination = {
  pageSize: 10,
}

const rules = {
  name: [{ required: true, message: '请输入角色名称', trigger: 'blur' }],
}

const transferListStyle = {
  width: '45%',
  height: '320px',
}

const tableData = computed(() => roles.value)

function filterPermission(inputValue: string, item: TransferOption) {
  return (item.title || '').toLowerCase().includes(inputValue.toLowerCase())
}

function renderPermissionItem(item: TransferOption) {
  return item.title || ''
}

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

async function fetchAllPermissions() {
  try {
    const permissionList = await PermissionsApi.getAllPermissions()
    permissions.value = Array.isArray(permissionList) ? permissionList : []
  } catch (error) {
    console.error('获取权限列表失败:', error)
    feedbackMessage.error('获取权限列表失败')
  }
}

function handleCreate() {
  isEdit.value = false
  currentRole.value = defaultRole()
  showModal.value = true
}

function handleEdit(role: Role) {
  isEdit.value = true
  currentRole.value = {
    ...role,
    permissionIds: role.permissions?.map((permission) => permission.id) ?? [],
  }
  showModal.value = true
}

function handleDelete(role: Role) {
  confirm({
    title: '删除角色',
    content: `确定要删除角色「${role.name}」吗？`,
    okType: 'danger',
    onOk: async () => {
      try {
        await RolesApi.deleteRole(role.id)
        feedbackMessage.success('删除成功')
        await fetchRoles()
      } catch (error) {
        console.error('删除失败:', error)
        feedbackMessage.error('删除失败')
      }
    },
  })
}

async function handleSubmit() {
  try {
    await formRef.value?.validate()
  } catch {
    return
  }

  submitLoading.value = true

  try {
    if (isEdit.value) {
      const updateData: UpdateRoleDto = {
        name: currentRole.value.name,
        description: currentRole.value.description,
        isEnabled: currentRole.value.isEnabled,
      }
      await RolesApi.updateRole(currentRole.value.id, updateData)
      await RolesApi.assignRolePermissions(currentRole.value.id, {
        permissionIds: currentRole.value.permissionIds,
      })
      feedbackMessage.success('更新成功')
    } else {
      const createData: CreateRoleDto = {
        name: currentRole.value.name,
        description: currentRole.value.description,
        isEnabled: currentRole.value.isEnabled,
        permissionIds: currentRole.value.permissionIds,
      }
      await RolesApi.createRole(createData)
      feedbackMessage.success('创建成功')
    }
    showModal.value = false
    await fetchRoles()
  } catch (error) {
    console.error(isEdit.value ? '更新失败:' : '创建失败:', error)
    feedbackMessage.error(isEdit.value ? '更新失败' : '创建失败')
  } finally {
    submitLoading.value = false
  }
}

onMounted(async () => {
  await Promise.all([fetchRoles(), fetchAllPermissions()])
})
</script>

<style scoped>
.role-management {
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.role-management__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
}

.role-management__header h1 {
  margin: 0;
  font-size: 24px;
  font-weight: 600;
}
</style>
