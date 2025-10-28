<template>
  <div class="captcha-container">
    <a-form-item
      :label="label"
      :name="fieldName"
      :rules="[
        { required: true, message: '请输入验证码' }
      ]"
    >
      <a-input-group compact>
        <a-input
          v-model:value="captchaCode"
          :placeholder="placeholder"
          :size="size"
          style="width: calc(100% - 120px)"
          @blur="validateCaptcha"
          @press-enter="$emit('press-enter')"
        />
        <div class="captcha-image-wrapper">
          <img
            :src="captchaImage"
            :alt="captchaAlt"
            class="captcha-image"
            @click="refreshCaptcha"
            :title="'点击刷新验证码'"
          />
          <a-spin v-if="loading" class="captcha-loading" size="small" />
        </div>
      </a-input-group>
      <div class="captcha-tips">
        <span>点击图片刷新验证码</span>
      </div>
    </a-form-item>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch } from 'vue'
import { message } from 'ant-design-vue'
import { captchaApi } from '@/api/modules/common'
import type { CaptchaResponse } from '@/types/modules/common'

interface Props {
  captchaCode?: string
  captchaValid?: boolean
  label?: string
  placeholder?: string
  size?: 'small' | 'middle' | 'large'
  fieldName?: string
  autoRefresh?: boolean
}

interface Emits {
  (e: 'update:captchaCode', value: string): void
  (e: 'update:captchaValid', value: boolean): void
  (e: 'press-enter'): void
}

const props = withDefaults(defineProps<Props>(), {
  label: '验证码',
  placeholder: '请输入验证码',
  size: 'middle',
  fieldName: 'captchaCode',
  autoRefresh: false
})

const emit = defineEmits<Emits>()

// 响应式数据
const captchaCode = ref('')
const captchaId = ref('')
const captchaImage = ref('')
const loading = ref(false)
const captchaAlt = ref('验证码')

// 生成验证码
const generateCaptcha = async () => {
  try {
    loading.value = true
    const response = await captchaApi.generateCaptcha()

    captchaId.value = response.captchaId
    captchaImage.value = response.image
    captchaCode.value = ''

    emit('update:captchaCode', '')
    emit('update:captchaValid', false)
  } catch (error: any) {
    console.error('生成验证码失败:', error)
    message.error('生成验证码失败，请稍后重试')
  } finally {
    loading.value = false
  }
}

// 刷新验证码
const refreshCaptcha = () => {
  generateCaptcha()
}

// 验证验证码
const validateCaptcha = async () => {
  if (!captchaCode.value.trim()) {
    emit('update:captchaValid', false)
    return
  }

  try {
    await captchaApi.validateCaptcha({
      captchaId: captchaId.value,
      code: captchaCode.value
    })

    emit('update:captchaValid', true)
  } catch (error: any) {
    console.error('验证码验证失败:', error)
    emit('update:captchaValid', false)
    message.error('验证码错误，请重新输入')
    refreshCaptcha()
  }
}

// 监听 props 变化
watch(() => props.captchaCode, (newValue) => {
  if (newValue !== captchaCode.value) {
    captchaCode.value = newValue || ''
  }
})

// 监听验证码输入变化
watch(captchaCode, (newValue) => {
  emit('update:captchaCode', newValue)
})

// 组件挂载时生成验证码
onMounted(() => {
  generateCaptcha()
})

// 自动刷新验证码
watch(() => props.autoRefresh, (autoRefresh) => {
  if (autoRefresh) {
    const interval = setInterval(() => {
      if (document.visibilityState === 'visible') {
        refreshCaptcha()
      }
    }, 5 * 60 * 1000) // 5分钟刷新一次

    onUnmounted(() => {
      clearInterval(interval)
    })
  }
})

// 暴露方法给父组件
defineExpose({
  refreshCaptcha,
  validateCaptcha,
  getCaptchaData: () => ({
    captchaId: captchaId.value,
    captchaCode: captchaCode.value
  })
})
</script>

<style scoped>
.captcha-container {
  width: 100%;
}

.captcha-image-wrapper {
  position: relative;
  width: 110px;
  height: 32px;
  display: inline-block;
  vertical-align: top;
  margin-left: 2px;
}

.captcha-image {
  width: 100%;
  height: 100%;
  border: 1px solid #d9d9d9;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.3s;
  object-fit: cover;
}

.captcha-image:hover {
  border-color: #4096ff;
  transform: scale(1.02);
}

.captcha-loading {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
}

.captcha-tips {
  font-size: 12px;
  color: #8c8c8c;
  margin-top: 4px;
  line-height: 1.5;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .captcha-image-wrapper {
    width: 100px;
    height: 28px;
  }

  .captcha-tips {
    font-size: 11px;
  }
}

/* 深色主题适配 */
[data-theme='dark'] .captcha-image {
  border-color: #434343;
}

[data-theme='dark'] .captcha-image:hover {
  border-color: #4096ff;
}

[data-theme='dark'] .captcha-tips {
  color: #a6a6a6;
}
</style>