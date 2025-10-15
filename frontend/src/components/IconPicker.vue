<template>
  <div class="icon-picker">
    <div class="icon-picker__control">
      <div class="icon-picker__preview" :class="{ 'icon-picker__preview--empty': !selectedIcon }">
        <n-icon v-if="selectedIcon" size="22">
          <component :is="selectedIcon" />
        </n-icon>
        <span v-else>未选择</span>
      </div>
      <n-input
        class="icon-picker__input"
        :value="modelValue ?? ''"
        placeholder="请选择图标"
        readonly
      />
      <n-button secondary type="primary" @click="showDrawer = true">选择</n-button>
      <n-button v-if="modelValue" quaternary type="error" @click="handleClear">清除</n-button>
    </div>

    <n-drawer v-model:show="showDrawer" width="720">
      <n-drawer-content title="选择图标">
        <div class="icon-picker__drawer-header">
          <n-input
            v-model:value="query"
            clearable
            placeholder="搜索图标名称，如 home"
            autofocus
          />
          <span class="icon-picker__count">共 {{ filteredIcons.length }} 个图标</span>
        </div>

        <n-grid :cols="6" :x-gap="12" :y-gap="12" responsive="screen" class="icon-picker__grid">
          <template v-for="option in filteredIcons" :key="option.name">
            <n-gi>
              <button
                type="button"
                class="icon-picker__item"
                :class="{ 'icon-picker__item--active': option.name === modelValue }"
                @click="handleSelect(option.name)"
              >
                <n-icon size="24">
                  <component :is="option.component" />
                </n-icon>
                <span>{{ option.name }}</span>
              </button>
            </n-gi>
          </template>
        </n-grid>
      </n-drawer-content>
    </n-drawer>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, withDefaults } from 'vue'
import { NButton, NDrawer, NDrawerContent, NGrid, NGi, NIcon, NInput } from 'naive-ui'
import type { Component } from 'vue'
import { getIconComponent, getAllIconOptions } from '@/utils/iconRegistry'

const props = withDefaults(
  defineProps<{
    modelValue?: string | null
  }>(),
  {
    modelValue: null,
  },
)

const emit = defineEmits<{
  (e: 'update:modelValue', value: string | null): void
}>()

const showDrawer = ref(false)
const query = ref('')

const iconOptions = getAllIconOptions()

const filteredIcons = computed(() => {
  const keyword = query.value.trim().toLowerCase()
  if (!keyword) {
    return iconOptions
  }
  return iconOptions.filter((option) => option.name.toLowerCase().includes(keyword))
})

const selectedIcon = computed<Component | null>(() => getIconComponent(props.modelValue))

const handleSelect = (name: string) => {
  emit('update:modelValue', name)
  showDrawer.value = false
}

const handleClear = () => {
  emit('update:modelValue', null)
}
</script>

<style scoped>
.icon-picker {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.icon-picker__control {
  display: flex;
  align-items: center;
  gap: 12px;
}

.icon-picker__preview {
  width: 64px;
  height: 40px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border: 1px solid var(--n-border-color);
  border-radius: 6px;
  background-color: var(--n-color);
  color: var(--n-text-color);
}

.icon-picker__preview--empty {
  font-size: 12px;
  color: var(--n-text-color-disabled);
}

.icon-picker__input {
  flex: 1;
}

.icon-picker__drawer-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 16px;
  gap: 12px;
}

.icon-picker__count {
  font-size: 12px;
  color: var(--n-text-color-disabled);
}

.icon-picker__grid {
  max-height: 60vh;
  overflow-y: auto;
  padding-right: 8px;
}

.icon-picker__item {
  width: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 12px;
  border: 1px solid transparent;
  border-radius: 8px;
  background-color: transparent;
  cursor: pointer;
  transition: all 0.2s ease;
  color: var(--n-text-color);
}

.icon-picker__item:hover {
  border-color: var(--n-border-color);
  background-color: var(--n-color);
}

.icon-picker__item--active {
  border-color: var(--n-primary-color);
  background-color: var(--n-primary-color-suppl);
  color: var(--n-primary-color-hover);
}

.icon-picker__item span {
  font-size: 12px;
  text-align: center;
  word-break: break-all;
}
</style>
