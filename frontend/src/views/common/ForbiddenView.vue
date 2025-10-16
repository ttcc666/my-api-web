<template>
  <div class="error-container">
    <a-card class="error-card" title="403 - 禁止访问">
      <a-result
        status="403"
        title="403 Forbidden"
        sub-title="抱歉，您没有权限访问此页面。"
      >
        <template #extra>
          <a-button type="primary" @click="goHome">返回首页</a-button>
        </template>
      </a-result>
    </a-card>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'

const router = useRouter()

function goHome() {
  router.push('/')
}
</script>

<style scoped>
.error-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background-color: #f5f5f5;
  padding: 24px;
  box-sizing: border-box;
}

.error-card {
  width: 450px;
  max-width: 100%;
  text-align: center;
}
</style>
