<template>
  <div class="login-container">
    <a-card title="登录" class="login-card">
      <a-form ref="formRef" layout="vertical" :model="formState" :rules="rules">
        <a-form-item name="username" label="用户名">
          <a-input v-model:value="formState.username" placeholder="请输入用户名" />
        </a-form-item>
        <a-form-item name="password" label="密码">
          <a-input-password v-model:value="formState.password" placeholder="请输入密码" />
        </a-form-item>

        <!-- 验证码组件 -->
        <CaptchaInput
          v-model:captchaCode="formState.captchaCode"
          v-model:captchaValid="captchaValid"
          @press-enter="handleLogin"
        />

        <a-form-item>
          <a-button type="primary" block :loading="loading" @click="handleLogin">登录</a-button>
        </a-form-item>
      </a-form>
      <div class="register-link">还没有账号？<RouterLink to="/register">立即注册</RouterLink></div>
    </a-card>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref } from 'vue'
import type { FormInstance } from 'ant-design-vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/modules/auth/auth'
import { useFeedback } from '@/composables/useFeedback'
import CaptchaInput from '@/components/common/CaptchaInput.vue'

defineOptions({
  name: 'UserLogin',
})

const formRef = ref<FormInstance>()
const formState = reactive({
  username: '',
  password: '',
  captchaCode: '',
})

const captchaValid = ref(false)

const rules = {
  username: [{ required: true, message: '请输入用户名', trigger: 'blur' }],
  password: [{ required: true, message: '请输入密码', trigger: 'blur' }],
  captchaCode: [
    { required: true, message: '请输入验证码', trigger: 'blur' },
    {
      validator: () => {
        return captchaValid.value || Promise.reject('验证码错误')
      },
      trigger: 'blur'
    }
  ],
}

const authStore = useAuthStore()
const router = useRouter()
const loading = ref(false)
const { message: feedbackMessage } = useFeedback()

async function handleLogin(event: MouseEvent) {
  event.preventDefault()
  try {
    await formRef.value?.validate()
  } catch {
    return
  }

  // 验证验证码
  if (!captchaValid.value) {
    feedbackMessage.error('请输入正确的验证码')
    return
  }

  loading.value = true
  const success = await authStore.login({
    username: formState.username,
    password: formState.password,
    captchaCode: formState.captchaCode,
  })
  loading.value = false

  if (success) {
    feedbackMessage.success('登录成功')
    router.push('/')
  } else {
    feedbackMessage.error(authStore.error || '登录失败')
    // 登录失败时刷新验证码
    const captchaComponent = formRef.value?.querySelector?.('captcha-input')
    if (captchaComponent?.refreshCaptcha) {
      captchaComponent.refreshCaptcha()
    }
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
