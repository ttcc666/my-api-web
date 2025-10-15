<script setup lang="ts">
import { ref, computed } from 'vue'
import { useUserStore } from '@/stores/user'
import { UsersApi } from '@/api'
import { message } from '@/plugins/naive'
import type { ChangePasswordDto, UserUpdateDto } from '@/types/api'

const userStore = useUserStore()
const user = computed(() => userStore.user)

const activeTab = ref('info')
const editMode = ref(false)
const loading = ref(false)

const formData = ref<UserUpdateDto>({
  realName: user.value?.realName || '',
  phone: user.value?.phone || '',
})

interface PasswordForm {
  currentPassword: string
  newPassword: string
  confirmPassword: string
}

const passwordForm = ref<PasswordForm>({
  currentPassword: '',
  newPassword: '',
  confirmPassword: '',
})
const passwordLoading = ref(false)

const handleEdit = () => {
  formData.value = {
    realName: user.value?.realName || '',
    phone: user.value?.phone || '',
  }
  editMode.value = true
}

const handleCancel = () => {
  editMode.value = false
}

const handleSave = async () => {
  if (!user.value?.id) return

  loading.value = true
  try {
    const payload: UserUpdateDto = {}

    const trimmedRealName = formData.value.realName?.trim()
    const trimmedPhone = formData.value.phone?.trim()

    if (trimmedRealName) {
      payload.realName = trimmedRealName
    }

    if (trimmedPhone) {
      payload.phone = trimmedPhone
    }

    await UsersApi.updateUser(user.value.id, payload)
    await userStore.fetchUserInfo()
    message.success('更新成功')
    editMode.value = false
  } catch {
    message.error('更新失败')
  } finally {
    loading.value = false
  }
}

const handlePasswordChange = async () => {
  if (!user.value?.id) return

  if (!passwordForm.value.currentPassword || !passwordForm.value.newPassword) {
    message.error('请输入当前密码和新密码')
    return
  }

  if (passwordForm.value.newPassword.length < 6) {
    message.error('新密码长度至少为 6 位')
    return
  }

  if (passwordForm.value.newPassword === passwordForm.value.currentPassword) {
    message.error('新密码不能与当前密码相同')
    return
  }

  if (passwordForm.value.newPassword !== passwordForm.value.confirmPassword) {
    message.error('两次输入的新密码不一致')
    return
  }

  const payload: ChangePasswordDto = {
    currentPassword: passwordForm.value.currentPassword,
    newPassword: passwordForm.value.newPassword,
  }

  passwordLoading.value = true
  try {
    await UsersApi.changePassword(user.value.id, payload)
    message.success('密码修改成功')
    passwordForm.value = {
      currentPassword: '',
      newPassword: '',
      confirmPassword: '',
    }
  } catch (error: any) {
    const errorMessage = error?.response?.data?.message ?? '修改密码失败'
    message.error(errorMessage)
  } finally {
    passwordLoading.value = false
  }
}
</script>

<template>
  <div class="profile-container">
    <n-card title="个人中心" :bordered="false">
      <n-grid :cols="24" :x-gap="24">
        <n-gi :span="6">
          <div class="profile-sidebar">
            <n-space vertical align="center" :size="16">
              <n-avatar :size="120" round :style="{ backgroundColor: '#18a058' }">
                {{ user?.username?.charAt(0).toUpperCase() }}
              </n-avatar>
              <n-h2 style="margin: 0">{{ user?.username }}</n-h2>
              <n-text depth="3">{{ user?.email }}</n-text>
              <n-tag v-if="user?.isActive" type="success">活跃</n-tag>
              <n-tag v-else type="error">未激活</n-tag>
            </n-space>
          </div>
        </n-gi>

        <n-gi :span="18">
          <n-tabs v-model:value="activeTab" type="line" animated>
            <n-tab-pane name="info" tab="基本信息">
              <n-space vertical :size="24">
                <n-descriptions v-if="!editMode" label-placement="left" :column="1" bordered>
                  <n-descriptions-item label="用户名">
                    {{ user?.username }}
                  </n-descriptions-item>
                  <n-descriptions-item label="邮箱">
                    {{ user?.email }}
                  </n-descriptions-item>
                  <n-descriptions-item label="真实姓名">
                    {{ user?.realName || '未设置' }}
                  </n-descriptions-item>
                  <n-descriptions-item label="手机号">
                    {{ user?.phone || '未设置' }}
                  </n-descriptions-item>
                  <n-descriptions-item label="注册时间">
                    {{
                      user?.createdTime ? new Date(user.createdTime).toLocaleString('zh-CN') : '-'
                    }}
                  </n-descriptions-item>
                  <n-descriptions-item label="最后登录">
                    {{
                      user?.lastLoginTime
                        ? new Date(user.lastLoginTime).toLocaleString('zh-CN')
                        : '-'
                    }}
                  </n-descriptions-item>
                </n-descriptions>

                <n-form v-else :model="formData" label-placement="left" label-width="100">
                  <n-form-item label="真实姓名">
                    <n-input v-model:value="formData.realName" placeholder="请输入真实姓名" />
                  </n-form-item>
                  <n-form-item label="手机号">
                    <n-input v-model:value="formData.phone" placeholder="请输入手机号" />
                  </n-form-item>
                </n-form>

                <n-space v-if="!editMode">
                  <n-button type="primary" @click="handleEdit">编辑资料</n-button>
                </n-space>
                <n-space v-else>
                  <n-button type="primary" :loading="loading" @click="handleSave">保存</n-button>
                  <n-button @click="handleCancel">取消</n-button>
                </n-space>
              </n-space>
            </n-tab-pane>

            <n-tab-pane name="security" tab="账户安全">
              <n-card title="修改密码" :bordered="false">
                <n-form :model="passwordForm" label-placement="left" label-width="100">
                  <n-form-item label="当前密码">
                    <n-input
                      v-model:value="passwordForm.currentPassword"
                      type="password"
                      placeholder="请输入当前密码"
                      show-password-on="click"
                    />
                  </n-form-item>
                  <n-form-item label="新密码">
                    <n-input
                      v-model:value="passwordForm.newPassword"
                      type="password"
                      placeholder="请输入新密码"
                      show-password-on="click"
                    />
                  </n-form-item>
                  <n-form-item label="确认密码">
                    <n-input
                      v-model:value="passwordForm.confirmPassword"
                      type="password"
                      placeholder="请再次输入新密码"
                      show-password-on="click"
                    />
                  </n-form-item>
                  <n-form-item>
                    <n-button type="primary" :loading="passwordLoading" @click="handlePasswordChange">
                      修改密码
                    </n-button>
                  </n-form-item>
                </n-form>
              </n-card>
            </n-tab-pane>
          </n-tabs>
        </n-gi>
      </n-grid>
    </n-card>
  </div>
</template>

<style scoped>
.profile-container {
  padding: 24px;
}

.profile-sidebar {
  padding: 24px;
  text-align: center;
}
</style>
