<template>
  <div class="login-container">
    <a-card title="登录" class="login-card">
      <a-form
        ref="formRef"
        layout="vertical"
        :model="formState"
        :rules="rules"
      >
        <a-form-item name="username" label="用户名">
          <a-input v-model:value="formState.username" placeholder="请输入用户名" />
        </a-form-item>
        <a-form-item name="password" label="密码">
          <a-input-password v-model:value="formState.password" placeholder="请输入密码" />
        </a-form-item>
        <a-form-item>
          <a-button type="primary" block :loading="loading" @click="handleLogin">登录</a-button>
        </a-form-item>
      </a-form>
      <div class="register-link">
        还没有账号？<RouterLink to="/register">立即注册</RouterLink>
      </div>
    </a-card>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref } from 'vue'
import type { FormInstance } from 'ant-design-vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { message } from '@/plugins/antd'

defineOptions({
  name: 'UserLogin',
})

const formRef = ref<FormInstance>()
const formState = reactive({
  username: '',
  password: '',
})

const rules = {
  username: [{ required: true, message: '请输入用户名', trigger: 'blur' }],
  password: [{ required: true, message: '请输入密码', trigger: 'blur' }],
}

const authStore = useAuthStore()
const router = useRouter()
const loading = ref(false)

async function handleLogin(event: MouseEvent) {
  event.preventDefault()
  try {
    await formRef.value?.validate()
  } catch {
    return
  }

  loading.value = true
  const success = await authStore.login({
    username: formState.username,
    password: formState.password,
  })
  loading.value = false

  if (success) {
    message.success('登录成功')
    router.push('/')
  } else {
    message.error(authStore.error || '登录失败')
  }
}
</script>

<style scoped>
.login-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background-color: #f5f5f5;
  padding: 24px;
  box-sizing: border-box;
}

.login-card {
  width: 420px;
  max-width: 100%;
}

.register-link {
  margin-top: 16px;
  text-align: center;
  color: rgba(0, 0, 0, 0.45);
}

.register-link a {
  margin-left: 4px;
}
</style>
