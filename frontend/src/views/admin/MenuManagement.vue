
<template>
  <div class="menu-management">
    <div class="menu-management__header">
      <h1>菜单管理</h1>
      <a-space>
        <a-button :loading="loading" @click="handleRefresh">刷新</a-button>
        <a-button type="primary" @click="handleCreateRoot">新增菜单</a-button>
      </a-space>
    </div>

    <a-table
      :columns="columns"
      :data-source="tableData"
      :loading="loading"
      :pagination="false"
      row-key="id"
      :defaultExpandAllRows="true"
    >
      <template #bodyCell="{ column, record }">
        <template v-if="column.key === 'type'">
          <a-tag :color="record.type === MenuType.Directory ? 'blue' : 'green'">
            {{ record.type === MenuType.Directory ? '目录' : '页面' }}
          </a-tag>
        </template>
        <template v-else-if="column.key === 'permissionCode'">
          {{ record.permissionCode || '-' }}
        </template>
        <template v-else-if="column.key === 'isEnabled'">
          <a-tag :color="record.isEnabled ? 'green' : 'red'">
            {{ record.isEnabled ? '启用' : '禁用' }}
          </a-tag>
        </template>
        <template v-else-if="column.key === 'actions'">
          <a-space>
            <a-button
              v-if="record.type === MenuType.Directory"
              type="link"
              @click="handleAddChild(record)"
            >
              新增子菜单
            </a-button>
            <a-button type="link" @click="handleEdit(record)">编辑</a-button>
            <a-button type="link" danger @click="handleDelete(record)">删除</a-button>
          </a-space>
        </template>
      </template>
    </a-table>

    <a-modal
      v-model:open="showModal"
      :title="modalTitle"
      width="720px"
      :confirm-loading="submitLoading"
      @ok="handleSubmit"
    >
      <a-form ref="formRef" layout="vertical" :model="form" :rules="rules">
        <a-row :gutter="[24, 12]">
          <a-col :xs="24" :md="12">
            <a-form-item name="title" label="名称">
              <a-input v-model:value="form.title" placeholder="请输入菜单名称" />
            </a-form-item>
          </a-col>
          <a-col :xs="24" :md="12">
            <a-form-item name="code" label="编码">
              <a-input
                v-model:value="form.code"
                placeholder="请输入唯一编码"
                :disabled="!isCreateMode"
              />
            </a-form-item>
          </a-col>
          <a-col :xs="24" :md="12">
            <a-form-item name="type" label="类型">
              <a-select v-model:value="form.type" :options="typeOptions" />
            </a-form-item>
          </a-col>
          <a-col :xs="24" :md="12">
            <a-form-item name="parentId" label="上级菜单">
              <a-tree-select
                v-model:value="form.parentId"
                :tree-data="parentOptions"
                placeholder="选择上级菜单"
                allow-clear
                :disabled="parentOptions.length === 0"
              />
            </a-form-item>
          </a-col>
          <a-col :xs="24" :md="12">
            <a-form-item name="icon" label="图标">
              <icon-picker v-model="form.icon" />
            </a-form-item>
          </a-col>
          <a-col :xs="24" :md="12">
            <a-form-item name="order" label="排序">
              <a-input-number v-model:value="form.order" :min="0" class="w-100" />
            </a-form-item>
          </a-col>
          <a-col :xs="24" :md="12">
            <a-form-item name="routeName" label="路由名称">
              <a-input
                v-model:value="form.routeName"
                placeholder="menu-management"
                :disabled="isDirectory"
              />
            </a-form-item>
          </a-col>
          <a-col :xs="24" :md="12">
            <a-form-item name="routePath" label="路由路径">
              <a-input
                v-model:value="form.routePath"
                placeholder="/admin/menus"
                :disabled="isDirectory"
              />
            </a-form-item>
          </a-col>
          <a-col :xs="24" :md="12">
            <a-form-item name="permissionCode" label="权限编码">
              <a-input
                v-model:value="form.permissionCode"
                placeholder="menu:view"
                :disabled="isDirectory"
              />
            </a-form-item>
          </a-col>
          <a-col :xs="24" :md="12">
            <a-form-item name="isEnabled" label="启用">
              <a-switch
                v-model:checked="form.isEnabled"
                checked-children="启用"
                un-checked-children="禁用"
              />
            </a-form-item>
          </a-col>
          <a-col :span="24">
            <a-form-item name="description" label="描述">
              <a-textarea v-model:value="form.description" :rows="3" />
            </a-form-item>
          </a-col>
        </a-row>
      </a-form>
    </a-modal>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import type { ColumnsType } from 'ant-design-vue/es/table'
import type { FormInstance } from 'ant-design-vue'
import { MenuType, type MenuDto, type CreateMenuDto, type UpdateMenuDto } from '@/types/api'
import { useMenuManagement } from '@/composables/useMenuManagement'
import IconPicker from '@/components/IconPicker.vue'
import { message, modal } from '@/plugins/antd'

interface MenuForm {
  id: string | null
  code: string
  title: string
  routePath: string
  routeName: string
  icon: string | null
  parentId: string | null
  order: number
  isEnabled: boolean
  type: MenuType
  permissionCode: string
  description: string
}

interface TreeNode {
  title: string
  value: string
  key: string
  disabled?: boolean
  children?: TreeNode[]
}

const {
  menuTree,
  loading: loadingRef,
  error,
  fetchMenuTree,
  createMenu,
  updateMenu,
  deleteMenu,
} = useMenuManagement()

const loading = computed(() => loadingRef.value)
const tableData = computed(() => menuTree.value)

const modalMode = ref<'create' | 'edit'>('create')
const showModal = ref(false)
const submitLoading = ref(false)

const formRef = ref<FormInstance>()
const form = ref<MenuForm>({
  id: null,
  code: '',
  title: '',
  routePath: '',
  routeName: '',
  icon: null,
  parentId: null,
  order: 0,
  isEnabled: true,
  type: MenuType.Route,
  permissionCode: '',
  description: '',
})

const typeOptions = [
  { label: '目录', value: MenuType.Directory },
  { label: '页面', value: MenuType.Route },
]

const rules = {
  title: [{ required: true, message: '请输入菜单名称', trigger: 'blur' }],
  code: [
    {
      async validator(_rule: unknown, value: string) {
        if (!isCreateMode.value) {
          return Promise.resolve()
        }
        if (!value || !value.trim()) {
          return Promise.reject(new Error('请输入唯一编码'))
        }
        return Promise.resolve()
      },
      trigger: 'blur',
    },
  ],
}

const isCreateMode = computed(() => modalMode.value === 'create')
const isDirectory = computed(() => form.value.type === MenuType.Directory)

const parentOptions = computed<TreeNode[]>(() => buildParentOptions(menuTree.value, form.value.id))
const modalTitle = computed(() => (isCreateMode.value ? '新增菜单' : '编辑菜单'))

const columns: ColumnsType<MenuDto> = [
  { title: '名称', dataIndex: 'title', key: 'title' },
  { title: '类型', key: 'type' },
  { title: '路由路径', dataIndex: 'routePath', key: 'routePath' },
  { title: '权限编码', key: 'permissionCode' },
  { title: '排序', dataIndex: 'order', key: 'order' },
  { title: '状态', key: 'isEnabled' },
  { title: '操作', key: 'actions', width: 260 },
]

watch(error, (err) => {
  if (err) {
    message.error(err)
  }
})

watch(
  () => form.value.type,
  (type) => {
    if (type === MenuType.Directory) {
      form.value.routePath = ''
      form.value.routeName = ''
      form.value.permissionCode = ''
    }
  },
)

function resetForm() {
  form.value = {
    id: null,
    code: '',
    title: '',
    routePath: '',
    routeName: '',
    icon: null,
    parentId: null,
    order: 0,
    isEnabled: true,
    type: MenuType.Route,
    permissionCode: '',
    description: '',
  }
  formRef.value?.clearValidate()
}

function handleCreateRoot() {
  resetForm()
  modalMode.value = 'create'
  showModal.value = true
}

function handleAddChild(menu: MenuDto) {
  resetForm()
  form.value.parentId = menu.id
  modalMode.value = 'create'
  showModal.value = true
}

function handleEdit(menu: MenuDto) {
  form.value = {
    id: menu.id,
    code: menu.code,
    title: menu.title,
    routePath: menu.routePath ?? '',
    routeName: menu.routeName ?? '',
    icon: menu.icon ?? null,
    parentId: menu.parentId ?? null,
    order: menu.order ?? 0,
    isEnabled: menu.isEnabled,
    type: menu.type,
    permissionCode: menu.permissionCode ?? '',
    description: menu.description ?? '',
  }
  modalMode.value = 'edit'
  showModal.value = true
}

function handleDelete(menu: MenuDto) {
  modal.confirm({
    title: '删除菜单',
    content: `确定删除菜单「${menu.title}」吗？`,
    okType: 'danger',
    onOk: async () => {
      try {
        await deleteMenu(menu.id)
        message.success('删除成功')
      } catch (err) {
        message.error(err instanceof Error ? err.message : '删除失败')
      }
    },
  })
}

async function handleRefresh() {
  try {
    await fetchMenuTree()
    message.success('刷新成功')
  } catch (err) {
    message.error(err instanceof Error ? err.message : '刷新失败')
  }
}

async function handleSubmit() {
  try {
    await formRef.value?.validate()
  } catch {
    return
  }

  submitLoading.value = true

  const payloadBase = {
    title: form.value.title.trim(),
    routePath: normalizeNullable(form.value.routePath),
    routeName: normalizeNullable(form.value.routeName),
    icon: normalizeNullable(form.value.icon),
    parentId: form.value.parentId || null,
    order: form.value.order ?? 0,
    isEnabled: form.value.isEnabled,
    type: form.value.type,
    permissionCode: normalizeNullable(form.value.permissionCode),
    description: normalizeNullable(form.value.description),
  }

  try {
    if (modalMode.value === 'create') {
      const payload: CreateMenuDto = {
        code: form.value.code.trim(),
        ...payloadBase,
      }
      await createMenu(payload)
      message.success('创建成功')
    } else if (form.value.id) {
      const payload: UpdateMenuDto = {
        ...payloadBase,
      }
      await updateMenu(form.value.id, payload)
      message.success('更新成功')
    }
    showModal.value = false
  } catch (err) {
    message.error(err instanceof Error ? err.message : '保存失败')
  } finally {
    submitLoading.value = false
  }
}

onMounted(async () => {
  await fetchMenuTree()
})

function normalizeNullable(value: string | null | undefined): string | null {
  if (value === null || value === undefined) {
    return null
  }
  const trimmed = value.trim()
  return trimmed.length ? trimmed : null
}

function buildParentOptions(menus: MenuDto[], excludeId?: string | null): TreeNode[] {
  return menus.reduce<TreeNode[]>((acc, menu) => {
    if (menu.id === excludeId) {
      return acc
    }
    const node: TreeNode = {
      title: menu.title,
      value: menu.id,
      key: menu.id,
      disabled: menu.type !== MenuType.Directory,
    }
    if (menu.children && menu.children.length > 0) {
      const children = buildParentOptions(menu.children, excludeId)
      if (children.length) {
        node.children = children
      }
    }
    acc.push(node)
    return acc
  }, [])
}
</script>

<style scoped>
.menu-management {
  display: flex;
  flex-direction: column;
  gap: 16px;
  padding: 24px;
}

.menu-management__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
}

.menu-management__header h1 {
  margin: 0;
  font-size: 24px;
  font-weight: 600;
}

.w-100 {
  width: 100%;
}
</style>
