<template>
  <div class="menu-management">
    <div class="page-header">
      <h1>菜单管理</h1>
      <div class="header-actions">
        <n-button quaternary :loading="loading" @click="handleRefresh">刷新</n-button>
        <n-button type="primary" @click="handleCreateRoot">新增菜单</n-button>
      </div>
    </div>

    <n-data-table
      :columns="columns"
      :data="menuTree"
      :loading="loading"
      :row-key="rowKey"
      default-expand-all
      :bordered="false"
    />

    <n-modal v-model:show="showModal" preset="card" :title="modalTitle" style="width: 720px">
      <n-form ref="formRef" :model="form" :rules="rules" label-placement="top">
        <n-grid cols="2" x-gap="24" y-gap="12">
          <n-form-item-gi label="名称" path="title">
            <n-input v-model:value="form.title" placeholder="请输入菜单名称" />
          </n-form-item-gi>
          <n-form-item-gi label="编码" path="code">
            <n-input
              v-model:value="form.code"
              placeholder="请输入唯一编码"
              :disabled="!isCreateMode"
            />
          </n-form-item-gi>
          <n-form-item-gi label="类型" path="type">
            <n-select v-model:value="form.type" :options="typeOptions" />
          </n-form-item-gi>
          <n-form-item-gi label="上级菜单" path="parentId">
            <n-tree-select
              v-model:value="form.parentId"
              :options="parentOptions"
              placeholder="选择上级菜单"
              clearable
              :disabled="parentOptions.length === 0"
            />
          </n-form-item-gi>
          <n-form-item-gi label="图标" path="icon">
            <icon-picker v-model="form.icon" />
          </n-form-item-gi>
          <n-form-item-gi label="排序" path="order">
            <n-input-number v-model:value="form.order" :min="0" />
          </n-form-item-gi>
          <n-form-item-gi label="路由名称" path="routeName">
            <n-input
              v-model:value="form.routeName"
              placeholder="menu-management"
              :disabled="isDirectory"
            />
          </n-form-item-gi>
          <n-form-item-gi label="路由路径" path="routePath">
            <n-input
              v-model:value="form.routePath"
              placeholder="/admin/menus"
              :disabled="isDirectory"
            />
          </n-form-item-gi>
          <n-form-item-gi label="权限编码" path="permissionCode">
            <n-input
              v-model:value="form.permissionCode"
              placeholder="menu:view"
              :disabled="isDirectory"
            />
          </n-form-item-gi>
          <n-form-item-gi label="启用" path="isEnabled">
            <n-switch v-model:value="form.isEnabled" />
          </n-form-item-gi>
          <n-form-item-gi span="2" label="描述" path="description">
            <n-input v-model:value="form.description" type="textarea" rows="3" />
          </n-form-item-gi>
        </n-grid>
      </n-form>
      <template #footer>
        <n-button @click="showModal = false">取消</n-button>
        <n-button type="primary" :loading="saving" @click="handleSubmit">保存</n-button>
      </template>
    </n-modal>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, h, onMounted, watch } from 'vue'
import {
  NButton,
  NDataTable,
  NForm,
  NFormItemGi,
  NGrid,
  NInput,
  NModal,
  NSwitch,
  NTreeSelect,
  NInputNumber,
  NTag,
  NSpace,
  useMessage,
  useDialog,
  type DataTableColumns,
  type FormInst,
  type FormRules,
  type TreeSelectOption,
} from 'naive-ui'
import { useMenuManagement } from '@/composables/useMenuManagement'
import IconPicker from '@/components/IconPicker.vue'
import { MenuType, type MenuDto, type CreateMenuDto, type UpdateMenuDto } from '@/types/api'

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

type ModalMode = 'create' | 'edit'

const message = useMessage()
const dialog = useDialog()

const {
  menuTree,
  loading: menuLoading,
  error,
  fetchMenuTree,
  createMenu,
  updateMenu,
  deleteMenu,
} = useMenuManagement()

const loading = computed(() => menuLoading.value)

const showModal = ref(false)
const modalMode = ref<ModalMode>('create')
const saving = ref(false)
const formRef = ref<FormInst | null>(null)

const createDefaultForm = (): MenuForm => ({
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

const form = ref<MenuForm>(createDefaultForm())

const modalTitle = computed(() => (modalMode.value === 'create' ? '新增菜单' : '编辑菜单'))
const isCreateMode = computed(() => modalMode.value === 'create')
const isDirectory = computed(() => form.value.type === MenuType.Directory)

const typeOptions = [
  { label: '目录', value: MenuType.Directory },
  { label: '页面', value: MenuType.Route },
]

const rules: FormRules = {
  title: [
    {
      required: true,
      trigger: ['blur', 'input'],
      message: '请输入菜单名称',
    },
  ],
  code: [
    {
      trigger: ['blur', 'input'],
      validator: (_rule, value: string) => {
        if (!isCreateMode.value) {
          return Promise.resolve()
        }
        if (!value || !value.trim()) {
          return Promise.reject('请输入菜单编码')
        }
        return Promise.resolve()
      },
    },
  ],
  type: [
    {
      trigger: ['change'],
      validator: (_rule, value: MenuType | null | undefined) => {
        if (value === null || value === undefined) {
          return Promise.reject('请选择菜单类型')
        }
        return Promise.resolve()
      },
    },
  ],
  routeName: [
    {
      trigger: ['blur', 'input'],
      validator: (_rule, value: string) => {
        if (isDirectory.value) {
          return Promise.resolve()
        }
        if (!value || !value.trim()) {
          return Promise.reject('请输入路由名称')
        }
        return Promise.resolve()
      },
    },
  ],
  routePath: [
    {
      trigger: ['blur', 'input'],
      validator: (_rule, value: string) => {
        if (isDirectory.value) {
          return Promise.resolve()
        }
        if (!value || !value.trim()) {
          return Promise.reject('请输入路由路径')
        }
        if (!value.startsWith('/')) {
          return Promise.reject('路由路径需以 / 开头')
        }
        return Promise.resolve()
      },
    },
  ],
}

const columns: DataTableColumns<MenuDto> = [
  {
    title: '名称',
    key: 'title',
  },
  {
    title: '类型',
    key: 'type',
    render(row) {
      const typeLabel = row.type === MenuType.Directory ? '目录' : '页面'
      const tagType = row.type === MenuType.Directory ? 'default' : 'success'
      return h(NTag, { type: tagType as 'default' | 'success' }, { default: () => typeLabel })
    },
  },
  {
    title: '路由路径',
    key: 'routePath',
  },
  {
    title: '权限编码',
    key: 'permissionCode',
    render(row) {
      return row.permissionCode || '-'
    },
  },
  {
    title: '排序',
    key: 'order',
  },
  {
    title: '状态',
    key: 'isEnabled',
    render(row) {
      return h(
        NTag,
        { type: row.isEnabled ? 'success' : 'error' },
        { default: () => (row.isEnabled ? '启用' : '禁用') },
      )
    },
  },
  {
    title: '操作',
    key: 'actions',
    render(row) {
      const actions = [] as Array<ReturnType<typeof h>>

      if (row.type === MenuType.Directory) {
        actions.push(
          h(
            NButton,
            {
              size: 'small',
              tertiary: true,
              onClick: () => handleAddChild(row),
            },
            { default: () => '新增子菜单' },
          ),
        )
      }

      actions.push(
        h(
          NButton,
          {
            size: 'small',
            type: 'primary',
            onClick: () => handleEdit(row),
          },
          { default: () => '编辑' },
        ),
      )

      actions.push(
        h(
          NButton,
          {
            size: 'small',
            type: 'error',
            secondary: true,
            onClick: () => handleDelete(row),
          },
          { default: () => '删除' },
        ),
      )

      return h(NSpace, { size: 'small' }, { default: () => actions })
    },
  },
]

const parentOptions = computed<TreeSelectOption[]>(() =>
  buildParentOptions(menuTree.value, form.value.id),
)

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

onMounted(async () => {
  try {
    await fetchMenuTree()
  } catch (err) {
    console.error('加载菜单失败:', err)
  }
})

const rowKey = (row: MenuDto) => row.id

function resetForm() {
  form.value = { ...createDefaultForm() }
}

function normalizeNullable(value: string | null | undefined): string | null {
  if (value === null || value === undefined) {
    return null
  }
  const trimmed = value.trim()
  return trimmed.length ? trimmed : null
}

function buildParentOptions(menus: MenuDto[], excludeId?: string | null): TreeSelectOption[] {
  const result: TreeSelectOption[] = []
  menus.forEach((menu) => {
    if (menu.id === excludeId) {
      return
    }

    const option: TreeSelectOption = {
      label: menu.title,
      key: menu.id,
      disabled: menu.type !== MenuType.Directory,
    }

    if (menu.children && menu.children.length > 0) {
      const children = buildParentOptions(menu.children, excludeId)
      if (children.length) {
        option.children = children
      }
    }

    result.push(option)
  })
  return result
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
  dialog.warning({
    title: '删除菜单',
    content: `确定删除菜单「${menu.title}」吗？`,
    positiveText: '删除',
    negativeText: '取消',
    onPositiveClick: async () => {
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
  if (!formRef.value) return

  try {
    await formRef.value.validate()
  } catch {
    return
  }

  saving.value = true

  const payloadBase = {
    title: form.value.title.trim(),
    routePath: normalizeNullable(form.value.routePath),
    routeName: normalizeNullable(form.value.routeName),
    icon: normalizeNullable(form.value.icon),
    parentId: form.value.parentId ? form.value.parentId : null,
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
    saving.value = false
  }
}
</script>

<style scoped>
.menu-management {
  display: flex;
  flex-direction: column;
  gap: 16px;
  padding: 16px;
}

.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.header-actions {
  display: flex;
  gap: 12px;
}

.table-actions {
  display: inline-flex;
  gap: 16px;
}
</style>
