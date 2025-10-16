<template>
  <div class="app-breadcrumb">
    <a-breadcrumb>
      <a-breadcrumb-item v-for="item in items" :key="item.key">
        <RouterLink v-if="item.to" :to="item.to" class="breadcrumb-link">
          <component :is="item.icon" v-if="item.icon" />
          <span>{{ item.label }}</span>
        </RouterLink>
        <span v-else class="breadcrumb-text">
          <component :is="item.icon" v-if="item.icon" />
          <span>{{ item.label }}</span>
        </span>
      </a-breadcrumb-item>
    </a-breadcrumb>
  </div>
</template>

<script setup lang="ts">
import { computed, h } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import { HomeOutlined } from '@ant-design/icons-vue'
import { useThemeStore } from '@/stores/modules/common/theme'

interface BreadcrumbItem {
  key: string
  label: string
  to?: { name: string }
  icon?: () => ReturnType<typeof h>
}

interface BreadcrumbMetaItem {
  title: string
  name?: string
}

const route = useRoute()
const themeStore = useThemeStore()

const items = computed<BreadcrumbItem[]>(() => {
  const result: BreadcrumbItem[] = [
    {
      key: 'home',
      label: '主页',
      to: route.name !== 'home' ? { name: 'home' } : undefined,
      icon: () => h(HomeOutlined),
    },
  ]

  const breadcrumbMeta = (route.meta?.breadcrumb as BreadcrumbMetaItem[] | undefined) || []
  breadcrumbMeta.forEach((metaItem, index) => {
    if (!metaItem?.title) return
    const key = metaItem.name ? String(metaItem.name) : `breadcrumb-${index}-${metaItem.title}`
    if (result.some((item) => item.key === key)) return
    result.push({
      key,
      label: metaItem.title,
      to: metaItem.name ? { name: metaItem.name } : undefined,
    })
  })

  if (route.name && route.name !== 'home' && typeof route.meta?.title === 'string') {
    const key = String(route.name)
    if (result.some((item) => item.key === key)) {
      return result
    }
    result.push({
      key,
      label: route.meta.title,
    })
  }

  return result
})
</script>

<style scoped>
.app-breadcrumb {
  display: flex;
  align-items: center;
  min-height: 32px;
}

.breadcrumb-link,
.breadcrumb-text {
  display: inline-flex;
  align-items: center;
  gap: 6px;
}

.breadcrumb-link {
  color: #555;
  transition: color 0.3s;
}

.breadcrumb-link:hover {
  color: v-bind('themeStore.primaryColor');
}

.breadcrumb-text {
  color: #888;
}
</style>
