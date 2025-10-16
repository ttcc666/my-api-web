import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { ThemeConfig } from 'ant-design-vue/es/config-provider/context'

export type LayoutMode = 'side' | 'top' | 'mix'
export type FontSize = 'small' | 'medium' | 'large'

export const useThemeStore = defineStore(
  'theme',
  () => {
    const primaryColor = ref('#1890ff')
    const layoutMode = ref<LayoutMode>('side')
    const fontSize = ref<FontSize>('medium')

    const theme = computed<ThemeConfig>(() => {
      const fontSizeMap = { small: 12, medium: 14, large: 16 }
      return {
        token: {
          colorPrimary: primaryColor.value,
          fontSize: fontSizeMap[fontSize.value],
        },
      }
    })

    const setPrimaryColor = (color: string) => {
      primaryColor.value = color
    }

    const setLayoutMode = (mode: LayoutMode) => {
      layoutMode.value = mode
    }

    const setFontSize = (size: FontSize) => {
      fontSize.value = size
    }

    return {
      primaryColor,
      layoutMode,
      fontSize,
      theme,
      setPrimaryColor,
      setLayoutMode,
      setFontSize,
    }
  },
  {
    persist: {
      key: 'theme-settings',
      pick: ['primaryColor', 'layoutMode', 'fontSize'],
    },
  },
)
