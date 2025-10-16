<script setup lang="ts">
import { RouterView } from 'vue-router'
import { useThemeStore } from '@/stores/modules/common/theme'

const themeStore = useThemeStore()
</script>

<template>
  <a-config-provider :theme="themeStore.theme">
    <a-app>
      <RouterView />
    </a-app>
  </a-config-provider>
</template>

<style>
html,
body,
#app {
  height: 100%;
}

body {
  margin: 0;
  overflow: hidden;
  background-color: #f5f5f5;
}
</style>
