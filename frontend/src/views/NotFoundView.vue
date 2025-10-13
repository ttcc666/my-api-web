<template>
  <div class="error-container">
    <n-card title="404 - 页面未找到" class="error-card">
      <n-result
        status="404"
        title="404 Not Found"
        description="抱歉，您访问的页面不存在。"
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