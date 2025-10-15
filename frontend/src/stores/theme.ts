import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { ThemeConfig } from 'ant-design-vue/es/config-provider/context'

export type LayoutMode = 'side' | 'top' | 'mix'
export type FontSize = 'small' | 'medium' | 'large'

const STORAGE_KEY = 'theme-settings'

export const useThemeStore = defineStore('theme', () => {
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

    function setPrimaryColor(color: string) {
        primaryColor.value = color
        saveSettings()
    }

    function setLayoutMode(mode: LayoutMode) {
        layoutMode.value = mode
        saveSettings()
    }

    function setFontSize(size: FontSize) {
        fontSize.value = size
        saveSettings()
    }

    function saveSettings() {
        localStorage.setItem(
            STORAGE_KEY,
            JSON.stringify({
                primaryColor: primaryColor.value,
                layoutMode: layoutMode.value,
                fontSize: fontSize.value,
            }),
        )
    }

    function loadSettings() {
        const stored = localStorage.getItem(STORAGE_KEY)
        if (stored) {
            try {
                const settings = JSON.parse(stored)
                primaryColor.value = settings.primaryColor || '#1890ff'
                layoutMode.value = settings.layoutMode || 'side'
                fontSize.value = settings.fontSize || 'medium'
            } catch {
                console.warn('Failed to load theme settings')
            }
        }
    }

    loadSettings()

    return {
        primaryColor,
        layoutMode,
        fontSize,
        theme,
        setPrimaryColor,
        setLayoutMode,
        setFontSize,
    }
})