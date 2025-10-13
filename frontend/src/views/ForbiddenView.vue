<template>
  <div class="error-container">
    <n-card title="403 - 禁止访问" class="error-card">
      <n-result
        status="403"
        title="403 Forbidden"
        description="抱歉，您没有权限访问此页面。"
      >
        <template #footer>
          <n-button type="primary" @click="goHome">返回首页</n-button>
        </template>
      </n-result>
    </n-card>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router';

const router = useRouter();

const goHome = () => {
  router.push('/');
};
</script>

<style scoped>
.error-container {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 100vh;
  background-color: #f0f2f5;
}
.error-card {
  width: 450px;
  text-align: center;
}
</style>