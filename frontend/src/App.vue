<script setup lang="ts">
import { RouterView } from 'vue-router'
</script>

<template>
  <a-config-provider>
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
