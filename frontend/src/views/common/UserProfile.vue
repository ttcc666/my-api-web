<script setup lang="ts">
import { computed, ref } from 'vue'
import { useUserStore } from '@/stores/modules/system/user'
import { UsersApi } from '@/api'
import { message } from '@/plugins/antd'
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

function handleEdit() {
  formData.value = {
    realName: user.value?.realName || '',
    phone: user.value?.phone || '',
  }
  editMode.value = true
}

function handleCancel() {
  editMode.value = false
}

async function handleSave() {
  if (!user.value?.id) {
    return
  }

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
  } catch (error) {
    console.error('更新用户信息失败:', error)
    message.error('更新失败')
  } finally {
    loading.value = false
  }
}

async function handlePasswordChange() {
  if (!user.value?.id) {
    return
  }

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
  } catch (error: unknown) {
    const errorMessage =
      (error as { response?: { data?: { message?: string } } })?.response?.data?.message ??
      '修改密码失败'
    message.error(errorMessage)
  } finally {
    passwordLoading.value = false
  }
}
</script>

<template>
  <div class="profile-container">
    <a-card title="个人中心" class="profile-card">
      <a-row :gutter="[24, 24]">
        <a-col :xs="24" :lg="6">
          <div class="profile-sidebar">
            <a-avatar :size="120" class="profile-avatar">
              {{ user?.username?.charAt(0).toUpperCase() }}
            </a-avatar>
            <div class="profile-username">{{ user?.username }}</div>
            <div class="profile-email">{{ user?.email }}</div>
            <a-tag v-if="user?.isActive" color="success">活跃</a-tag>
            <a-tag v-else color="error">未激活</a-tag>
          </div>
        </a-col>
        <a-col :xs="24" :lg="18">
          <a-tabs v-model:activeKey="activeTab">
            <a-tab-pane key="info" tab="基本信息">
              <div class="tab-content">
                <template v-if="!editMode">
                  <a-descriptions :column="1" bordered size="middle">
                    <a-descriptions-item label="用户名">{{ user?.username }}</a-descriptions-item>
                    <a-descriptions-item label="邮箱">{{ user?.email }}</a-descriptions-item>
                    <a-descriptions-item label="真实姓名">
                      {{ user?.realName || '未设置' }}
                    </a-descriptions-item>
                    <a-descriptions-item label="手机号">
                      {{ user?.phone || '未设置' }}
                    </a-descriptions-item>
                    <a-descriptions-item label="注册时间">
                      {{
                        user?.createdTime ? new Date(user.createdTime).toLocaleString('zh-CN') : '-'
                      }}
                    </a-descriptions-item>
                    <a-descriptions-item label="最后登录">
                      {{
                        user?.lastLoginTime
                          ? new Date(user.lastLoginTime).toLocaleString('zh-CN')
                          : '-'
                      }}
                    </a-descriptions-item>
                  </a-descriptions>
                  <div class="action-group">
                    <a-button type="primary" @click="handleEdit">编辑资料</a-button>
                  </div>
                </template>
                <template v-else>
                  <a-form layout="vertical" :model="formData">
                    <a-form-item label="真实姓名">
                      <a-input v-model:value="formData.realName" placeholder="请输入真实姓名" />
                    </a-form-item>
                    <a-form-item label="手机号">
                      <a-input v-model:value="formData.phone" placeholder="请输入手机号" />
                    </a-form-item>
                  </a-form>
                  <div class="action-group">
                    <a-button type="primary" :loading="loading" @click="handleSave">保存</a-button>
                    <a-button @click="handleCancel">取消</a-button>
                  </div>
                </template>
              </div>
            </a-tab-pane>
            <a-tab-pane key="security" tab="账户安全">
              <div class="tab-content">
                <div class="password-section">
                  <h3>修改密码</h3>
                  <a-form layout="vertical" :model="passwordForm">
                    <a-form-item label="当前密码">
                      <a-input-password
                        v-model:value="passwordForm.currentPassword"
                        placeholder="请输入当前密码"
                      />
                    </a-form-item>
                    <a-form-item label="新密码">
                      <a-input-password
                        v-model:value="passwordForm.newPassword"
                        placeholder="请输入新密码"
                      />
                    </a-form-item>
                    <a-form-item label="确认密码">
                      <a-input-password
                        v-model:value="passwordForm.confirmPassword"
                        placeholder="请再次输入新密码"
                      />
                    </a-form-item>
                    <a-button
                      type="primary"
                      :loading="passwordLoading"
                      @click="handlePasswordChange"
                    >
                      修改密码
                    </a-button>
                  </a-form>
                </div>
              </div>
            </a-tab-pane>
          </a-tabs>
        </a-col>
      </a-row>
    </a-card>
  </div>
</template>

<style scoped>
.profile-container {
  padding: 24px;
}

.profile-card {
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
}

.profile-sidebar {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
  padding: 16px;
  text-align: center;
}

.profile-avatar {
  background: linear-gradient(135deg, #1677ff 0%, #69c0ff 100%);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: 36px;
  font-weight: 600;
}

.profile-username {
  font-size: 20px;
  font-weight: 600;
  color: #1f1f1f;
}

.profile-email {
  color: rgba(0, 0, 0, 0.45);
}

.tab-content {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.action-group {
  display: flex;
  gap: 12px;
}

.password-section h3 {
  margin: 0 0 12px;
  font-size: 18px;
  font-weight: 600;
}

@media (max-width: 768px) {
  .profile-container {
    padding: 16px;
  }

  .action-group {
    flex-wrap: wrap;
  }
}
</style>
