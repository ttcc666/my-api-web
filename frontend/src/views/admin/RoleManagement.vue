<template>
  <div class="role-management">
    <h1>角色管理</h1>
    <n-button v-permission="'create:role'" type="primary" @click="handleCreate">
      创建角色
    </n-button>
    <n-data-table
      :columns="columns"
      :data="roles"
      :loading="loading"
      :pagination="pagination"
    />

    <!-- 创建/编辑角色的模态框 -->
    <n-modal v-model:show="showModal" preset="card" style="width: 600px" :title="modalTitle">
      <n-form ref="formRef" :model="currentRole" :rules="rules">
        <n-form-item path="name" label="角色名称">
          <n-input v-model:value="currentRole.name" :disabled="currentRole.isSystem" />
        </n-form-item>
        <n-form-item path="description" label="描述">
          <n-input v-model:value="currentRole.description" type="textarea" />
        </n-form-item>
        <n-form-item label="权限">
          <n-transfer
            v-model:value="currentRole.permissionIds"
            :options="permissionOptions"
            source-filterable
          />
        </n-form-item>
        <n-form-item v-if="isEdit" label="状态">
          <n-switch v-model:value="currentRole.isEnabled" :disabled="currentRole.isSystem" />
        </n-form-item>
      </n-form>
      <template #footer>
        <n-button @click="showModal = false">取消</n-button>
        <n-button type="primary" @click="handleSubmit">确定</n-button>
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
  NSwitch,
  NTransfer,
  useMessage,
  type DataTableColumns,
  type FormInst,
  type FormRules
} from 'naive-ui'
import RoleApi, { type RoleCreateDto, type RoleUpdateDto } from '@/api/role'
import type { Role, Permission } from '@/types/api'

const message = useMessage()
const loading = ref(false)
const roles = ref<Role[]>([])
const permissions = ref<Permission[]>([])
const showModal = ref(false)
const isEdit = ref(false)
const formRef = ref<FormInst | null>(null)

const defaultRole = (): Role & { permissionIds: string[] } => ({
  id: '',
  name: '',
  description: '',
  isSystem: false,
  isEnabled: true,
  creationTime: '',
  permissions: [],
  permissionIds: []
})

const currentRole = ref(defaultRole())

const modalTitle = computed(() => (isEdit.value ? '编辑角色' : '创建角色'))

const permissionOptions = computed(() =>
  permissions.value.map(p => ({
    label: p.displayName,
    value: p.id,
    disabled: false
  }))
)

const rules: FormRules = {
  name: [{ required: true, message: '请输入角色名称', trigger: 'blur' }]
}

const columns: DataTableColumns<Role> = [
  { title: '角色名称', key: 'name' },
  { title: '描述', key: 'description' },
  { title: '系统角色', key: 'isSystem', render: row => (row.isSystem ? '是' : '否') },
  { title: '状态', key: 'isEnabled', render: row => (row.isEnabled ? '启用' : '禁用') },
  {
    title: '操作',
    key: 'actions',
    render(row) {
      return h('div', [
        h(
          NButton,
          {
            size: 'small',
            type: 'primary',
            onClick: () => handleEdit(row),
            'v-permission': "'edit:role'"
          },
          { default: () => '编辑' }
        ),
        h(
          NButton,
          {
            size: 'small',
            type: 'error',
            style: 'margin-left: 8px',
            disabled: row.isSystem,
            onClick: () => handleDelete(row),
            'v-permission': "'delete:role'"
          },
          { default: () => '删除' }
        )
      ])
    }
  }
]

const pagination = {
  pageSize: 10
}

async function fetchRoles() {
  try {
    loading.value = true
    const roleList = await RoleApi.getAllRoles();
    console.log('Fetched roles:', roleList); // 添加日志
    if (Array.isArray(roleList)) {
      roles.value = roleList;
    } else {
      console.error('Expected an array of roles, but got:', roleList);
      roles.value = [];
    }
  } catch (error) {
    message.error('获取角色列表失败')
  } finally {
    loading.value = false
  }
}

async function fetchPermissions() {
  try {
    const permissionList = await RoleApi.getAllPermissions();
    permissions.value = Array.isArray(permissionList) ? permissionList : [];
  } catch (error) {
    message.error('获取权限列表失败')
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
    permissionIds: role.permissions.map(p => p.id)
  }
  showModal.value = true
}

async function handleDelete(role: Role) {
  try {
    await RoleApi.deleteRole(role.id)
    message.success('删除成功')
    await fetchRoles()
  } catch (error) {
    message.error('删除失败')
  }
}

async function handleSubmit() {
  formRef.value?.validate(async errors => {
    if (!errors) {
      try {
        if (isEdit.value) {
          const updateData: RoleUpdateDto = {
            name: currentRole.value.name,
            description: currentRole.value.description,
            isEnabled: currentRole.value.isEnabled,
            permissionIds: currentRole.value.permissionIds
          }
          await RoleApi.updateRole(currentRole.value.id, updateData)
          message.success('更新成功')
        } else {
          const createData: RoleCreateDto = {
            name: currentRole.value.name,
            description: currentRole.value.description,
            permissionIds: currentRole.value.permissionIds
          }
          await RoleApi.createRole(createData)
          message.success('创建成功')
        }
        showModal.value = false
        await fetchRoles()
      } catch (error) {
        message.error(isEdit.value ? '更新失败' : '创建失败')
      }
    }
  })
}

onMounted(() => {
  fetchRoles()
  fetchPermissions()
})
</script>

<style scoped>
.role-management {
  padding: 20px;
}
</style>