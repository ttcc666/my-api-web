<template>
  <div class="online-user-management">
    <!-- 顶部工具栏 -->
    <div class="online-user-management__header">
      <h1>在线用户管理</h1>
      <a-space>
        <a-select
          v-model:value="refreshIntervalValue"
          :options="intervalOptions"
          @change="handleIntervalChange"
          style="width: 120px"
          :disabled="loading"
        />
        <a-switch
          v-model:checked="autoRefreshEnabled"
          @change="handleAutoRefreshToggle"
          :disabled="loading"
        >
          <template #checkedChildren>自动</template>
          <template #unCheckedChildren>手动</template>
        </a-switch>
        <a-button type="primary" :loading="loading" @click="handleRefresh">
          <template #icon><ReloadOutlined /></template>
          刷新
        </a-button>
        <a-button :loading="loading" @click="handleCleanupClick" v-if="hasAdminPermission">
          <template #icon><ClearOutlined /></template>
          清理超时连接
        </a-button>
      </a-space>
    </div>

    <a-spin :spinning="loading || statisticsLoading">
      <a-alert v-if="error" type="error" :message="error" show-icon style="margin-bottom: 16px" />

      <!-- 统计信息卡片 -->
      <a-row :gutter="16" class="statistics-cards" v-if="statistics">
        <!-- 总在线用户数 -->
        <a-col :xs="24" :sm="12" :lg="6">
          <a-card>
            <a-statistic
              title="总在线用户数"
              :value="statistics.totalOnlineUsers"
              :value-style="{ color: '#1890ff' }"
            >
              <template #prefix><UserOutlined /></template>
            </a-statistic>
          </a-card>
        </a-col>

        <!-- 总连接数 -->
        <a-col :xs="24" :sm="12" :lg="6">
          <a-card>
            <a-statistic
              title="总连接数"
              :value="statistics.totalConnections"
              :value-style="{ color: '#52c41a' }"
            >
              <template #prefix><LinkOutlined /></template>
            </a-statistic>
          </a-card>
        </a-col>

        <!-- 活跃用户 -->
        <a-col :xs="24" :sm="12" :lg="6">
          <a-card>
            <a-statistic
              title="活跃用户"
              :value="statistics.activeUsers"
              :value-style="{ color: '#52c41a' }"
            >
              <template #prefix><CheckCircleOutlined /></template>
            </a-statistic>
            <div class="card-footer">空闲: {{ statistics.idleUsers }}</div>
          </a-card>
        </a-col>

        <!-- 平均在线时长 -->
        <a-col :xs="24" :sm="12" :lg="6">
          <a-card>
            <a-statistic
              title="平均在线时长"
              :value="formatDuration(statistics.averageOnlineDurationSeconds)"
            >
              <template #prefix><ClockCircleOutlined /></template>
            </a-statistic>
            <div class="card-footer">今日峰值: {{ statistics.todayPeakUsers }}</div>
          </a-card>
        </a-col>
      </a-row>

      <!-- 筛选工具栏 -->
      <a-card class="filter-card">
        <a-form layout="inline">
          <a-form-item label="状态">
            <a-select
              v-model:value="filterStatus"
              style="width: 120px"
              @change="handleFilterChange"
              allowClear
              placeholder="全部"
            >
              <a-select-option value="Online">在线</a-select-option>
              <a-select-option value="Idle">空闲</a-select-option>
              <a-select-option value="Offline">离线</a-select-option>
            </a-select>
          </a-form-item>
          <a-form-item label="用户ID">
            <a-input
              v-model:value="filterUserId"
              style="width: 200px"
              @pressEnter="handleFilterChange"
              @blur="handleFilterChange"
              placeholder="输入用户ID"
              allowClear
            />
          </a-form-item>
          <a-form-item label="房间">
            <a-input
              v-model:value="filterRoom"
              style="width: 200px"
              @pressEnter="handleFilterChange"
              @blur="handleFilterChange"
              placeholder="输入房间名称"
              allowClear
            />
          </a-form-item>
        </a-form>
      </a-card>

      <!-- 在线用户表格 -->
      <a-card class="table-card">
        <a-table
          :columns="columns"
          :data-source="onlineUsers"
          :loading="loading"
          :pagination="pagination"
          row-key="id"
          @change="handleTableChange"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'username'">
              <a-tag color="blue">{{ record.username || '未知' }}</a-tag>
            </template>
            <template v-else-if="column.key === 'status'">
              <a-tag :color="getStatusColor(record.status)">
                {{ getStatusText(record.status) }}
              </a-tag>
            </template>
            <template v-else-if="column.key === 'onlineDuration'">
              {{ formatDuration(record.onlineDurationSeconds || 0) }}
            </template>
            <template v-else-if="column.key === 'connectedAt'">
              {{ formatDateTime(record.connectedAt) }}
            </template>
            <template v-else-if="column.key === 'lastHeartbeatAt'">
              {{ formatDateTime(record.lastHeartbeatAt) }}
            </template>
            <template v-else-if="column.key === 'actions'">
              <a-button
                type="link"
                danger
                size="small"
                :disabled="record.status !== 'Online'"
                @click="handleForceDisconnectClick(record)"
              >
                强制下线
              </a-button>
            </template>
          </template>
        </a-table>
      </a-card>
    </a-spin>

    <!-- 强制下线确认对话框 -->
    <a-modal
      v-model:open="showDisconnectModal"
      title="强制下线确认"
      @ok="handleForceDisconnectConfirm"
      :confirm-loading="disconnecting"
    >
      <p>
        确定要强制下线用户 <strong>{{ selectedUser?.username || '未知' }}</strong> 吗?
      </p>
      <a-form-item label="下线原因">
        <a-textarea v-model:value="disconnectReason" :rows="3" placeholder="请输入下线原因(可选)" />
      </a-form-item>
    </a-modal>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import {
  ReloadOutlined,
  ClearOutlined,
  UserOutlined,
  LinkOutlined,
  CheckCircleOutlined,
  ClockCircleOutlined,
} from '@ant-design/icons-vue'
import type { TableProps } from 'ant-design-vue'
import { useOnlineUsers } from '@/composables/useOnlineUsers'
import { usePermissionStore } from '@/stores/permission'
import type { OnlineUserDto, OnlineUserStatus } from '@/types/api'
import { message, modal } from '@/plugins/antd'

defineOptions({
  name: 'OnlineUserManagement',
})

// 使用 composable
const {
  onlineUsers,
  statistics,
  loading,
  statisticsLoading,
  error,
  pagination,
  autoRefreshEnabled,
  refreshInterval,
  loadOnlineUsers,
  loadStatistics,
  refreshAll,
  handlePageChange,
  handleFilterChange: applyFilters,
  handleForceDisconnect,
  handleCleanup,
  startAutoRefresh,
  stopAutoRefresh,
  setRefreshInterval,
  formatDuration,
  getStatusColor,
  getStatusText,
} = useOnlineUsers()

// 权限检查
const permissionStore = usePermissionStore()
const hasAdminPermission = computed(() => permissionStore.hasRole('Admin'))

// 筛选条件
const filterStatus = ref<OnlineUserStatus | undefined>()
const filterUserId = ref<string>()
const filterRoom = ref<string>()

// 刷新间隔选项
const refreshIntervalValue = ref(10000)
const intervalOptions = [
  { label: '5秒', value: 5000 },
  { label: '10秒', value: 10000 },
  { label: '30秒', value: 30000 },
  { label: '60秒', value: 60000 },
]

// 强制下线相关
const showDisconnectModal = ref(false)
const selectedUser = ref<OnlineUserDto | null>(null)
const disconnectReason = ref('')
const disconnecting = ref(false)

// 表格列定义
const columns: TableProps['columns'] = [
  {
    title: '用户名',
    dataIndex: 'username',
    key: 'username',
    width: 120,
  },
  {
    title: '用户ID',
    dataIndex: 'userId',
    key: 'userId',
    width: 150,
  },
  {
    title: '连接ID',
    dataIndex: 'connectionId',
    key: 'connectionId',
    width: 200,
    ellipsis: true,
  },
  {
    title: 'IP地址',
    dataIndex: 'ipAddress',
    key: 'ipAddress',
    width: 150,
  },
  {
    title: '状态',
    key: 'status',
    dataIndex: 'status',
    width: 100,
  },
  {
    title: '在线时长',
    key: 'onlineDuration',
    width: 150,
  },
  {
    title: '连接时间',
    key: 'connectedAt',
    dataIndex: 'connectedAt',
    width: 180,
  },
  {
    title: '最后心跳',
    key: 'lastHeartbeatAt',
    dataIndex: 'lastHeartbeatAt',
    width: 180,
  },
  {
    title: '房间',
    dataIndex: 'room',
    key: 'room',
    width: 120,
  },
  {
    title: '操作',
    key: 'actions',
    width: 120,
    fixed: 'right',
  },
]

// 格式化日期时间
const formatDateTime = (dateString: string): string => {
  const date = new Date(dateString)
  return date.toLocaleString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
  })
}

// 刷新数据
const handleRefresh = async (): Promise<void> => {
  await refreshAll()
}

// 处理表格变化
const handleTableChange: TableProps['onChange'] = (pag) => {
  if (pag.current && pag.pageSize) {
    handlePageChange(pag.current, pag.pageSize)
  }
}

// 处理筛选变化
const handleFilterChange = (): void => {
  applyFilters({
    status: filterStatus.value,
    userId: filterUserId.value,
    room: filterRoom.value,
  })
}

// 处理自动刷新切换
const handleAutoRefreshToggle = (checked: boolean): void => {
  if (checked) {
    startAutoRefresh()
  } else {
    stopAutoRefresh()
  }
}

// 处理刷新间隔变化
const handleIntervalChange = (value: number): void => {
  setRefreshInterval(value)
}

// 处理强制下线点击
const handleForceDisconnectClick = (user: OnlineUserDto): void => {
  selectedUser.value = user
  disconnectReason.value = ''
  showDisconnectModal.value = true
}

// 处理强制下线确认
const handleForceDisconnectConfirm = async (): Promise<void> => {
  if (!selectedUser.value) return

  try {
    disconnecting.value = true
    await handleForceDisconnect(
      selectedUser.value.connectionId,
      disconnectReason.value || undefined,
    )
    showDisconnectModal.value = false
  } catch (error) {
    console.error('强制下线失败:', error)
  } finally {
    disconnecting.value = false
  }
}

// 处理清理超时连接点击
const handleCleanupClick = (): void => {
  modal.confirm({
    title: '清理超时连接',
    content: '确定要清理所有超时连接吗?这将断开15分钟内未发送心跳的连接。',
    onOk: async () => {
      try {
        await handleCleanup(15)
      } catch (error) {
        console.error('清理失败:', error)
      }
    },
  })
}

// 组件挂载
onMounted(async () => {
  await refreshAll()
})
</script>

<style scoped>
.online-user-management {
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  background: #f0f2f5;
  min-height: calc(100vh - 64px);
}

.online-user-management__header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: white;
  padding: 16px 24px;
  border-radius: 8px;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.03);
}

.online-user-management__header h1 {
  margin: 0;
  font-size: 24px;
  font-weight: 600;
  color: rgba(0, 0, 0, 0.85);
}

/* 统计卡片 */
.statistics-cards {
  margin-bottom: 0;
}

.statistics-cards :deep(.ant-card) {
  border-radius: 8px;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.03);
  transition: all 0.3s ease;
}

.statistics-cards :deep(.ant-card:hover) {
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  transform: translateY(-2px);
}

.statistics-cards :deep(.ant-statistic-title) {
  font-size: 14px;
  color: rgba(0, 0, 0, 0.45);
  margin-bottom: 8px;
}

.statistics-cards :deep(.ant-statistic-content) {
  font-size: 24px;
  font-weight: 600;
}

.card-footer {
  margin-top: 12px;
  font-size: 12px;
  color: rgba(0, 0, 0, 0.45);
}

/* 筛选卡片 */
.filter-card {
  border-radius: 8px;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.03);
}

.filter-card :deep(.ant-card-body) {
  padding: 16px 24px;
}

/* 表格卡片 */
.table-card {
  border-radius: 8px;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.03);
}

.table-card :deep(.ant-table) {
  background: white;
}

/* 响应式 */
@media (max-width: 768px) {
  .online-user-management {
    padding: 16px;
  }

  .online-user-management__header {
    flex-direction: column;
    gap: 16px;
    align-items: flex-start;
  }

  .online-user-management__header h1 {
    font-size: 20px;
  }
}
</style>
