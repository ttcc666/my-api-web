<template>
  <div class="login-container">
    <n-card title="登录" class="login-card">
      <n-form ref="formRef" :model="model" :rules="rules">
        <n-form-item path="username" label="用户名">
          <n-input v-model:value="model.username" placeholder="请输入用户名" />
        </n-form-item>
        <n-form-item path="password" label="密码">
          <n-input
            v-model:value="model.password"
            type="password"
            show-password-on="mousedown"
            placeholder="请输入密码"
          />
        </n-form-item>
        <n-form-item>
          <n-button type="primary" @click="handleLogin" block :loading="loading">
            登录
          </n-button>
        </n-form-item>
      </n-form>
      <div class="register-link">
        还没有账号？<router-link to="/register">立即注册</router-link>
      </div>
    </n-card>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { message } from '@/plugins/naive'
import type { FormInst } from 'naive-ui'

const formRef = ref<FormInst | null>(null)
const model = ref({
  username: '',
  password: ''
})
const rules = {
  username: {
    required: true,
    message: '请输入用户名',
    trigger: 'blur'
  },
  password: {
    required: true,
    message: '请输入密码',
    trigger: 'blur'
  }
}

const authStore = useAuthStore()
const router = useRouter()
const loading = ref(false)

const handleLogin = (e: MouseEvent) => {
  e.preventDefault()
  formRef.value?.validate(async (errors) => {
    if (!errors) {
      loading.value = true
      const success = await authStore.login(model.value)
      loading.value = false
      if (success) {
        message.success('登录成功')
        router.push('/')
      } else {
        message.error(authStore.error || '登录失败')
      }
    }
  })
}
</script>

<style scoped>
.login-container {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 100vh;
  background-color: #f0f2f5;
}
.login-card {
  width: 400px;
}
.register-link {
  text-align: center;
  margin-top: 16px;
}
</style>