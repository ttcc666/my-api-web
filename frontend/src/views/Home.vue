<template>
  <n-space vertical :size="24" style="padding: 24px">
    <!-- 欢迎区域 -->
    <n-card title="欢迎使用 My App" :bordered="false" embedded>
      <template #header-extra>
        <n-tag type="success" round> 在线 </n-tag>
      </template>
      <n-space vertical :size="16">
        <n-h1 style="margin: 0; color: #18a058"> 欢迎回来！ </n-h1>
        <n-p style="margin: 0; font-size: 16px; color: #666">
          这是一个基于 .NET 9 和 Vue 3 的现代化前后端分离 Web 应用。
        </n-p>
        <n-p style="margin: 0; color: #999"> 您可以通过下方的快捷操作开始使用系统功能。 </n-p>
      </n-space>
    </n-card>

    <!-- 数据统计区域 -->
    <n-card title="数据概览" :bordered="false">
      <n-grid :cols="3" :x-gap="24">
        <n-grid-item>
          <n-statistic label="在线用户" :value="onlineUsers">
            <template #prefix>
              <n-icon color="#18a058">
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor">
                  <path
                    d="M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z"
                  />
                </svg>
              </n-icon>
            </template>
            <template #suffix>
              <n-text depth="3">人</n-text>
            </template>
          </n-statistic>
        </n-grid-item>
        <n-grid-item>
          <n-statistic label="待办事项" :value="todoCount">
            <template #prefix>
              <n-icon color="#f0a020">
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor">
                  <path
                    d="M19 3h-4.18C14.4 1.84 13.3 1 12 1c-1.3 0-2.4.84-2.82 2H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm-7 0c.55 0 1 .45 1 1s-.45 1-1 1-1-.45-1-1 .45-1 1-1zm2 14H7v-2h7v2zm3-4H7v-2h10v2zm0-4H7V7h10v2z"
                  />
                </svg>
              </n-icon>
            </template>
            <template #suffix>
              <n-text depth="3">项</n-text>
            </template>
          </n-statistic>
        </n-grid-item>
        <n-grid-item>
          <n-statistic label="消息通知" :value="messageCount">
            <template #prefix>
              <n-icon color="#d03050">
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor">
                  <path
                    d="M12 22c1.1 0 2-.9 2-2h-4c0 1.1.89 2 2 2zm6-6v-5c0-3.07-1.64-5.64-4.5-6.32V4c0-.83-.67-1.5-1.5-1.5s-1.5.67-1.5 1.5v.68C7.63 5.36 6 7.92 6 11v5l-2 2v1h16v-1l-2-2z"
                  />
                </svg>
              </n-icon>
            </template>
            <template #suffix>
              <n-text depth="3">条</n-text>
            </template>
          </n-statistic>
        </n-grid-item>
      </n-grid>
    </n-card>

    <!-- 快捷操作区域 -->
    <n-card title="快捷操作" :bordered="false">
      <n-grid :cols="2" :x-gap="24" :y-gap="16">
        <n-grid-item>
          <n-card
            hoverable
            style="cursor: pointer; height: 120px"
            @click="navigateToUserManagement"
          >
            <n-space align="center">
              <n-avatar
                :size="48"
                color="#18a058"
                style="background: linear-gradient(135deg, #18a058 0%, #36ad6a 100%)"
              >
                <n-icon :size="24">
                  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor">
                    <path
                      d="M16 4c0-1.11.89-2 2-2s2 .89 2 2-.89 2-2 2-2-.89-2-2zm4 18v-6h2.5l-2.54-7.63A1.5 1.5 0 0 0 18.54 8H17c-.8 0-1.54.37-2.01 1l-2.99 4v7h2v7h4zm-7.5-10.5c.83 0 1.5-.67 1.5-1.5s-.67-1.5-1.5-1.5S11 9.17 11 10.5s.67 1.5 1.5 1.5zM5.5 6c1.11 0 2-.89 2-2s-.89-2-2-2-2 .89-2 2 .89 2 2 2zm2 16v-7H9V9.5c0-.8-.67-1.5-1.5-1.5S6 8.7 6 9.5V15H4v7h3.5z"
                    />
                  </svg>
                </n-icon>
              </n-avatar>
              <n-space vertical :size="4">
                <n-text strong style="font-size: 16px">用户管理</n-text>
                <n-text depth="3" style="font-size: 14px">管理系统用户信息</n-text>
              </n-space>
            </n-space>
          </n-card>
        </n-grid-item>
        <n-grid-item>
          <n-card
            hoverable
            style="cursor: pointer; height: 120px"
            @click="navigateToRoleManagement"
          >
            <n-space align="center">
              <n-avatar
                :size="48"
                color="#f0a020"
                style="background: linear-gradient(135deg, #f0a020 0%, #fcb040 100%)"
              >
                <n-icon :size="24">
                  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor">
                    <path
                      d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"
                    />
                  </svg>
                </n-icon>
              </n-avatar>
              <n-space vertical :size="4">
                <n-text strong style="font-size: 16px">角色管理</n-text>
                <n-text depth="3" style="font-size: 14px">配置用户角色权限</n-text>
              </n-space>
            </n-space>
          </n-card>
        </n-grid-item>
      </n-grid>
    </n-card>

    <!-- 最近活动区域 -->
    <n-card title="最近活动" :bordered="false">
      <n-timeline>
        <n-timeline-item
          v-for="activity in recentActivities"
          :key="activity.id"
          :type="activity.type"
          :title="activity.title"
          :content="activity.content"
          :time="activity.time"
        />
      </n-timeline>
    </n-card>
  </n-space>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'

defineOptions({
  name: 'HomePage',
})

const router = useRouter()

// 模拟数据
const onlineUsers = ref(128)
const todoCount = ref(15)
const messageCount = ref(3)

const recentActivities = ref([
  {
    id: 1,
    type: 'success',
    title: '用户登录',
    content: '管理员 admin 登录系统',
    time: '2 分钟前',
  },
  {
    id: 2,
    type: 'info',
    title: '数据更新',
    content: '系统完成了定时数据同步',
    time: '1 小时前',
  },
  {
    id: 3,
    type: 'warning',
    title: '权限变更',
    content: '用户 john 的权限已被修改',
    time: '3 小时前',
  },
])

// 导航方法
const navigateToUserManagement = () => {
  router.push({ name: 'user-management' })
}

const navigateToRoleManagement = () => {
  router.push({ name: 'role-management' })
}
</script>

<style scoped>
/* 自定义样式 */
.n-card {
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  border-radius: 8px;
}

.n-card:hover {
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.15);
  transition: box-shadow 0.3s ease;
}

.n-statistic {
  text-align: center;
}
</style>
