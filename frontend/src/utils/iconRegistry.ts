import type { Component } from 'vue'
import * as Ionicons from '@vicons/ionicons5'

type IconEntry = [string, Component]

const rawEntries = Object.entries(Ionicons) as Array<[string, unknown]>

const iconEntries: IconEntry[] = rawEntries
  .filter(([, value]) => typeof value === 'object' || typeof value === 'function')
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
