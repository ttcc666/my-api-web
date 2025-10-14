<template>
  <div class="register-container">
    <n-card title="注册" class="register-card">
      <n-form ref="formRef" :model="model" :rules="rules">
        <n-form-item path="username" label="用户名">
          <n-input v-model:value="model.username" placeholder="请输入用户名" />
        </n-form-item>
        <n-form-item path="email" label="邮箱">
          <n-input v-model:value="model.email" placeholder="请输入邮箱" />
        </n-form-item>
        <n-form-item path="password" label="密码">
          <n-input
            v-model:value="model.password"
            type="password"
            show-password-on="mousedown"
            placeholder="请输入密码"
          />
        </n-form-item>
        <n-form-item path="repassword" label="确认密码">
          <n-input
            v-model:value="model.repassword"
            type="password"
            show-password-on="mousedown"
            placeholder="请再次输入密码"
          />
        </n-form-item>
        <n-form-item>
          <n-button type="primary" @click="handleRegister" block :loading="loading">
            注册
          </n-button>
        </n-form-item>
      </n-form>
      <div class="login-link">已有账号？<router-link to="/login">立即登录</router-link></div>
    </n-card>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { message } from '@/plugins/naive'
import type { FormInst, FormRules } from 'naive-ui'

defineOptions({
  name: 'UserRegister',
})

const formRef = ref<FormInst | null>(null)
const model = ref({
  username: '',
  email: '',
  password: '',
  repassword: '',
})

const validatePasswordSame = (_rule: unknown, value: string) => {
  return value === model.value.password
}

const rules: FormRules = {
  username: {
    required: true,
    message: '请输入用户名',
    trigger: 'blur',
  },
  email: {
    required: true,
    type: 'email',
    message: '请输入正确的邮箱地址',
    trigger: ['input', 'blur'],
  },
  password: {
    required: true,
    message: '请输入密码',
    trigger: 'blur',
  },
  repassword: [
    {
      required: true,
      message: '请再次输入密码',
      trigger: ['input', 'blur'],
    },
    {
      validator: validatePasswordSame,
      message: '两次输入的密码不一致',
      trigger: ['blur', 'password-input'],
    },
  ],
}

const authStore = useAuthStore()
const router = useRouter()
const loading = ref(false)

const handleRegister = (e: MouseEvent) => {
  e.preventDefault()
  formRef.value?.validate(async (errors) => {
    if (!errors) {
      loading.value = true
      const success = await authStore.register(model.value)
      loading.value = false
      if (success) {
        message.success('注册成功，请登录')
        router.push('/login')
      } else {
        message.error(authStore.error || '注册失败')
      }
    }
  })
}
</script>

<style scoped>
.register-container {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 100vh;
  background-color: #f0f2f5;
}
.register-card {
  width: 400px;
}
.login-link {
  text-align: center;
  margin-top: 16px;
}
</style>
