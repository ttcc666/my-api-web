import type { Component } from 'vue'
import * as AntIcons from '@ant-design/icons-vue'

type IconEntry = [string, Component]

const rawEntries = Object.entries(AntIcons) as Array<[string, unknown]>

const iconEntries: IconEntry[] = rawEntries
  .filter(([name, value]) => {
    // 过滤掉非图标组件（如 createFromIconfontCN, getTwoToneColor 等工具函数）
    if (typeof value !== 'object' && typeof value !== 'function') return false
    if (name === 'default') return false
    if (name.startsWith('create') || name.startsWith('get') || name.startsWith('set')) return false
    // 只保留以 Outlined, Filled, TwoTone 结尾的图标组件
    return name.endsWith('Outlined') || name.endsWith('Filled') || name.endsWith('TwoTone')
  })
  .map(([name, component]) => [name, component as Component])

iconEntries.sort((a, b) => a[0].localeCompare(b[0]))

const iconMap: Record<string, Component> = iconEntries.reduce<Record<string, Component>>((acc, [name, component]) => {
  acc[name] = component
  return acc
}, {})

export function getIconComponent(name?: string | null): Component | null {
  if (!name) {
    return null
  }
  return iconMap[name] ?? null
}

export function getAllIconOptions(): { name: string; component: Component }[] {
  return iconEntries.map(([name, component]) => ({ name, component }))
}
