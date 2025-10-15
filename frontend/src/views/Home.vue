<template>
  <div class="home-page">
    <a-card class="home-card welcome-card" :bordered="false">
      <div class="welcome-header">
        <div class="welcome-text">
          <h1>欢迎回来！</h1>
          <p class="welcome-subtitle">
            这是一个基于 .NET 9 和 Vue 3 的现代化前后端分离 Web 应用。
          </p>
          <p class="welcome-helper">您可以通过下方的快捷操作开始使用系统功能。</p>
        </div>
        <a-tag color="success">在线</a-tag>
      </div>
    </a-card>

    <a-card title="数据概览" class="home-card">
      <a-row :gutter="[24, 24]">
        <a-col :xs="24" :sm="12" :lg="8">
          <a-statistic title="在线用户" :value="onlineUsers">
            <template #prefix>
              <span class="stat-icon stat-icon--success">
                <svg viewBox="0 0 24 24" fill="currentColor">
                  <path
                    d="M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z"
                  />
                </svg>
              </span>
            </template>
            <template #suffix>
              <span class="stat-suffix">人</span>
            </template>
          </a-statistic>
        </a-col>
        <a-col :xs="24" :sm="12" :lg="8">
          <a-statistic title="待办事项" :value="todoCount">
            <template #prefix>
              <span class="stat-icon stat-icon--warning">
                <svg viewBox="0 0 24 24" fill="currentColor">
                  <path
                    d="M19 3h-4.18C14.4 1.84 13.3 1 12 1c-1.3 0-2.4.84-2.82 2H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm-7 0c.55 0 1 .45 1 1s-.45 1-1 1-1-.45-1-1 .45-1 1-1zm2 14H7v-2h7v2zm3-4H7v-2h10v2zm0-4H7V7h10v2z"
                  />
                </svg>
              </span>
            </template>
            <template #suffix>
              <span class="stat-suffix">项</span>
            </template>
          </a-statistic>
        </a-col>
        <a-col :xs="24" :sm="12" :lg="8">
          <a-statistic title="消息通知" :value="messageCount">
            <template #prefix>
              <span class="stat-icon stat-icon--danger">
                <svg viewBox="0 0 24 24" fill="currentColor">
                  <path
                    d="M12 22c1.1 0 2-.9 2-2h-4c0 1.1.89 2 2 2zm6-6v-5c0-3.07-1.64-5.64-4.5-6.32V4c0-.83-.67-1.5-1.5-1.5s-1.5.67-1.5 1.5v.68C7.63 5.36 6 7.92 6 11v5l-2 2v1h16v-1l-2-2z"
                  />
                </svg>
              </span>
            </template>
            <template #suffix>
              <span class="stat-suffix">条</span>
            </template>
          </a-statistic>
        </a-col>
      </a-row>
    </a-card>

    <a-card title="快捷操作" class="home-card">
      <a-row :gutter="[24, 24]">
        <a-col :xs="24" :md="12">
          <a-card hoverable class="action-card" @click="navigateToUserManagement">
            <div class="action-card__content">
              <a-avatar :size="48" class="action-card__avatar action-card__avatar--success">
                <svg viewBox="0 0 24 24" fill="currentColor">
                  <path
                    d="M16 4c0-1.11.89-2 2-2s2 .89 2 2-.89 2-2 2-2-.89-2-2zm4 18v-6h2.5l-2.54-7.63A1.5 1.5 0 0 0 18.54 8H17c-.8 0-1.54.37-2.01 1l-2.99 4v7h2v7h4zm-7.5-10.5c.83 0 1.5-.67 1.5-1.5s-.67-1.5-1.5-1.5S11 9.17 11 10.5s.67 1.5 1.5 1.5zM5.5 6c1.11 0 2-.89 2-2s-.89-2-2-2-2 .89-2 2 .89 2 2 2zm2 16v-7H9V9.5c0-.8-.67-1.5-1.5-1.5S6 8.7 6 9.5V15H4v7h3.5z"
                  />
                </svg>
              </a-avatar>
              <div class="action-card__text">
                <div class="action-card__title">用户管理</div>
                <div class="action-card__description">管理系统用户信息</div>
              </div>
            </div>
          </a-card>
        </a-col>
        <a-col :xs="24" :md="12">
          <a-card hoverable class="action-card" @click="navigateToRoleManagement">
            <div class="action-card__content">
              <a-avatar :size="48" class="action-card__avatar action-card__avatar--warning">
                <svg viewBox="0 0 24 24" fill="currentColor">
                  <path
                    d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"
                  />
                </svg>
              </a-avatar>
              <div class="action-card__text">
                <div class="action-card__title">角色管理</div>
                <div class="action-card__description">配置用户角色权限</div>
              </div>
            </div>
          </a-card>
        </a-col>
      </a-row>
    </a-card>

    <a-card title="最近活动" class="home-card">
      <a-timeline>
        <a-timeline-item
          v-for="activity in recentActivities"
          :key="activity.id"
          :color="activity.color"
        >
          <div class="timeline-item">
            <div class="timeline-title">{{ activity.title }}</div>
            <div class="timeline-content">{{ activity.content }}</div>
            <div class="timeline-time">{{ activity.time }}</div>
          </div>
        </a-timeline-item>
      </a-timeline>
    </a-card>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'

defineOptions({
  name: 'HomePage',
})

const router = useRouter()

const onlineUsers = ref(128)
const todoCount = ref(15)
const messageCount = ref(3)

const recentActivities = ref([
  {
    id: 1,
    color: 'green',
    title: '用户登录',
    content: '管理员 admin 登录系统',
    time: '2 分钟前',
  },
  {
    id: 2,
    color: 'blue',
    title: '数据更新',
    content: '系统完成了定时数据同步',
    time: '1 小时前',
  },
  {
    id: 3,
    color: 'orange',
    title: '权限变更',
    content: '用户 john 的权限已被修改',
    time: '3 小时前',
  },
])

function navigateToUserManagement() {
  router.push({ name: 'user-management' })
}

function navigateToRoleManagement() {
  router.push({ name: 'role-management' })
}
</script>

<style scoped>
.home-page {
  display: flex;
  flex-direction: column;
  gap: 24px;
  padding: 24px;
}

.home-card {
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
}

.welcome-card {
  padding: 24px;
}

.welcome-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
}

.welcome-text h1 {
  margin: 0 0 8px;
  font-size: 28px;
  color: #1677ff;
}

.welcome-subtitle {
  margin: 0 0 6px;
  font-size: 16px;
  color: #595959;
}

.welcome-helper {
  margin: 0;
  color: #8c8c8c;
}

.stat-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  border-radius: 50%;
  margin-right: 12px;
}

.stat-icon svg {
  width: 22px;
  height: 22px;
  color: #fff;
}

.stat-icon--success {
  background: linear-gradient(135deg, #52c41a 0%, #389e0d 100%);
}

.stat-icon--warning {
  background: linear-gradient(135deg, #fadb14 0%, #faad14 100%);
}

.stat-icon--danger {
  background: linear-gradient(135deg, #ff7875 0%, #ff4d4f 100%);
}

.stat-suffix {
  margin-left: 4px;
  color: rgba(0, 0, 0, 0.45);
}

.action-card {
  cursor: pointer;
  border-radius: 8px;
}

.action-card__content {
  display: flex;
  align-items: center;
  gap: 16px;
  min-height: 120px;
}

.action-card__avatar {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  color: #fff;
}

.action-card__avatar svg {
  width: 24px;
  height: 24px;
}

.action-card__avatar--success {
  background: linear-gradient(135deg, #52c41a 0%, #36cfc9 100%);
}

.action-card__avatar--warning {
  background: linear-gradient(135deg, #faad14 0%, #ffc53d 100%);
}

.action-card__text {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.action-card__title {
  font-size: 16px;
  font-weight: 600;
  color: #1f1f1f;
}

.action-card__description {
  font-size: 14px;
  color: rgba(0, 0, 0, 0.45);
}

.timeline-item {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.timeline-title {
  font-weight: 600;
  color: #1f1f1f;
}

.timeline-content {
  color: rgba(0, 0, 0, 0.65);
}

.timeline-time {
  font-size: 12px;
  color: rgba(0, 0, 0, 0.45);
}

@media (max-width: 768px) {
  .welcome-header {
    flex-direction: column;
    align-items: flex-start;
  }

  .home-page {
    padding: 16px;
  }
}
</style>
