<template>
  <div class="register-container">
    <a-card title="注册" class="register-card">
      <a-form ref="formRef" layout="vertical" :model="formState" :rules="rules">
        <a-form-item name="username" label="用户名">
          <a-input v-model:value="formState.username" placeholder="请输入用户名" />
        </a-form-item>
        <a-form-item name="email" label="邮箱">
          <a-input v-model:value="formState.email" placeholder="请输入邮箱" />
        </a-form-item>
        <a-form-item name="password" label="密码">
          <a-input-password v-model:value="formState.password" placeholder="请输入密码" />
        </a-form-item>
        <a-form-item name="repassword" label="确认密码">
          <a-input-password v-model:value="formState.repassword" placeholder="请再次输入密码" />
        </a-form-item>
        <a-form-item>
          <a-button type="primary" block :loading="loading" @click="handleRegister">
            注册
          </a-button>
        </a-form-item>
      </a-form>
      <div class="login-link">已有账号？<RouterLink to="/login">立即登录</RouterLink></div>
    </a-card>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref } from 'vue'
import type { FormInstance } from 'ant-design-vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/modules/auth/auth'
import { message } from '@/plugins/antd'

defineOptions({
  name: 'UserRegister',
})

const formRef = ref<FormInstance>()
const formState = reactive({
  username: '',
  email: '',
  password: '',
  repassword: '',
})

const rules = {
  username: [{ required: true, message: '请输入用户名', trigger: 'blur' }],
  email: [
    { required: true, message: '请输入邮箱地址', trigger: 'blur' },
    { type: 'email', message: '请输入正确的邮箱地址', trigger: ['blur', 'change'] },
  ],
  password: [{ required: true, message: '请输入密码', trigger: 'blur' }],
  repassword: [
    { required: true, message: '请再次输入密码', trigger: 'blur' },
    {
      validator: (_rule: unknown, value: string) => {
        if (!value) {
          return Promise.resolve()
        }
        if (value !== formState.password) {
          return Promise.reject('两次输入的密码不一致')
        }
        return Promise.resolve()
      },
      trigger: ['change', 'blur'],
    },
  ],
}

const authStore = useAuthStore()
const router = useRouter()
const loading = ref(false)

async function handleRegister(event: MouseEvent) {
  event.preventDefault()
  try {
    await formRef.value?.validate()
  } catch {
    return
  }

  loading.value = true
  const success = await authStore.register({
    username: formState.username,
    email: formState.email,
    password: formState.password,
  })
  loading.value = false

  if (success) {
    message.success('注册成功，请登录')
    router.push('/login')
  } else {
    message.error(authStore.error || '注册失败')
  }
}
</script>

<style scoped>
.register-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background-color: #f5f5f5;
  padding: 24px;
  box-sizing: border-box;
}

.register-card {
  width: 420px;
  max-width: 100%;
}

.login-link {
  margin-top: 16px;
  text-align: center;
  color: rgba(0, 0, 0, 0.45);
}

.login-link a {
  margin-left: 4px;
}
</style>
