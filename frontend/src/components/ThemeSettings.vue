<template>
  <a-drawer
    :open="visible"
    @update:open="emit('update:visible', $event)"
    title="主题设置"
    placement="right"
    :width="360"
  >
    <a-space direction="vertical" :size="32" style="width: 100%">
      <div class="setting-section">
        <div class="setting-label">
          <FormatPainterOutlined class="label-icon" />
          <span>主题色</span>
        </div>
        <div class="color-grid">
          <div
            v-for="color in presetColors"
            :key="color"
            class="color-block"
            :class="{ active: themeStore.primaryColor === color }"
            :style="{ backgroundColor: color }"
            @click="themeStore.setPrimaryColor(color)"
          >
            <CheckOutlined v-if="themeStore.primaryColor === color" class="check-icon" />
          </div>
        </div>
      </div>

      <a-divider style="margin: 0" />

      <div class="setting-section">
        <div class="setting-label">
          <LayoutOutlined class="label-icon" />
          <span>布局模式</span>
        </div>
        <a-radio-group
          v-model:value="themeStore.layoutMode"
          @change="handleLayoutChange"
          button-style="solid"
          style="width: 100%"
        >
          <a-radio-button value="side" style="width: 33.33%; text-align: center"
            >侧边</a-radio-button
          >
          <a-radio-button value="top" style="width: 33.33%; text-align: center"
            >顶部</a-radio-button
          >
          <a-radio-button value="mix" style="width: 33.33%; text-align: center"
            >混合</a-radio-button
          >
        </a-radio-group>
      </div>

      <a-divider style="margin: 0" />

      <div class="setting-section">
        <div class="setting-label">
          <FontSizeOutlined class="label-icon" />
          <span>字体大小</span>
        </div>
        <a-radio-group
          v-model:value="themeStore.fontSize"
          @change="handleFontSizeChange"
          button-style="solid"
          style="width: 100%"
        >
          <a-radio-button value="small" style="width: 33.33%; text-align: center"
            >小</a-radio-button
          >
          <a-radio-button value="medium" style="width: 33.33%; text-align: center"
            >中</a-radio-button
          >
          <a-radio-button value="large" style="width: 33.33%; text-align: center"
            >大</a-radio-button
          >
        </a-radio-group>
      </div>
    </a-space>
  </a-drawer>
</template>

<script setup lang="ts">
import {
  CheckOutlined,
  FormatPainterOutlined,
  LayoutOutlined,
  FontSizeOutlined,
} from '@ant-design/icons-vue'
import { useThemeStore } from '@/stores/modules/common/theme'
import type { LayoutMode, FontSize } from '@/stores/modules/common/theme'

defineProps<{
  visible: boolean
}>()

const emit = defineEmits<{
  'update:visible': [value: boolean]
}>()

const themeStore = useThemeStore()

const presetColors = [
  '#1890ff',
  '#722ed1',
  '#13c2c2',
  '#52c41a',
  '#eb2f96',
  '#f5222d',
  '#fa8c16',
  '#faad14',
]

function handleLayoutChange(e: { target: { value: LayoutMode } }) {
  themeStore.setLayoutMode(e.target.value)
}

function handleFontSizeChange(e: { target: { value: FontSize } }) {
  themeStore.setFontSize(e.target.value)
}
</script>

<style scoped>
.setting-section {
  width: 100%;
}

.setting-label {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 16px;
  font-size: 15px;
  font-weight: 600;
  color: rgba(0, 0, 0, 0.88);
}

.label-icon {
  font-size: 18px;
}

.color-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 12px;
}

.color-block {
  position: relative;
  width: 100%;
  aspect-ratio: 1;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  border: 3px solid transparent;
  display: flex;
  align-items: center;
  justify-content: center;
}

.color-block:hover {
  transform: scale(1.08);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.color-block.active {
  border-color: rgba(0, 0, 0, 0.15);
  box-shadow: 0 0 0 3px rgba(0, 0, 0, 0.08);
  transform: scale(1.05);
}

.check-icon {
  color: #fff;
  font-size: 20px;
  font-weight: bold;
  filter: drop-shadow(0 1px 2px rgba(0, 0, 0, 0.3));
}
</style>
