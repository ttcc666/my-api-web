<template>
  <div class="icon-picker">
    <div class="icon-picker__control">
      <div class="icon-picker__preview" :class="{ 'icon-picker__preview--empty': !selectedIcon }">
        <component v-if="selectedIcon" :is="selectedIcon" class="icon-picker__preview-icon" />
        <span v-else>未选择</span>
      </div>
      <a-input class="icon-picker__input" :value="modelValue ?? ''" placeholder="请选择图标" readonly />
      <a-button type="primary" @click="showDrawer = true">选择</a-button>
      <a-button v-if="modelValue" type="link" danger @click="handleClear">清除</a-button>
    </div>

    <a-drawer v-model:open="showDrawer" width="720" title="选择图标" @close="resetQuery">
      <div class="icon-picker__drawer-header">
        <a-input-search v-model:value="query" allow-clear autofocus placeholder="搜索图标名称，如 home" />
        <span class="icon-picker__count">共 {{ filteredIcons.length }} 个图标</span>
      </div>

      <div class="icon-picker__grid">
        <button v-for="option in filteredIcons" :key="option.name" type="button" class="icon-picker__item"
          :class="{ 'icon-picker__item--active': option.name === modelValue }" @click="handleSelect(option.name)">
          <component :is="option.component" class="icon-picker__item-icon" v-if="option.component" />
          <span class="icon-picker__item-name">{{ option.name }}</span>
        </button>
      </div>
    </a-drawer>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, withDefaults } from 'vue'
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

function resetQuery() {
  query.value = ''
}

const iconOptions = getAllIconOptions()

const filteredIcons = computed(() => {
  const keyword = query.value.trim().toLowerCase()
  if (!keyword) {
    return iconOptions
  }
  return iconOptions.filter((option) => option.name.toLowerCase().includes(keyword))
})

const selectedIcon = computed<Component | null>(() => getIconComponent(props.modelValue))

function handleSelect(name: string) {
  emit('update:modelValue', name)
  showDrawer.value = false
  resetQuery()
}

function handleClear() {
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
  border: 1px solid #d9d9d9;
  border-radius: 6px;
  background-color: #fafafa;
  color: rgba(0, 0, 0, 0.88);
}

.icon-picker__preview--empty {
  font-size: 12px;
  color: rgba(0, 0, 0, 0.45);
}

.icon-picker__preview-icon {
  font-size: 22px;
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
  color: rgba(0, 0, 0, 0.45);
}

.icon-picker__grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(100px, 1fr));
  gap: 12px;
  overflow-y: auto;
  padding-right: 4px;
}

.icon-picker__item {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 12px;
  border: 1px solid transparent;
  border-radius: 8px;
  background-color: #fff;
  cursor: pointer;
  transition: all 0.2s ease;
  color: rgba(0, 0, 0, 0.88);
}

.icon-picker__item:hover {
  border-color: #d9d9d9;
  background-color: #f5f5f5;
}

.icon-picker__item--active {
  border-color: #1677ff;
  background-color: #e6f4ff;
  color: #1677ff;
}

.icon-picker__item-icon {
  font-size: 24px;
}

.icon-picker__item-name {
  font-size: 12px;
  text-align: center;
  word-break: break-all;
  line-height: 1.4;
}
</style>
