<template>
  <div class="system-monitor">
    <!-- 顶部工具栏 -->
    <div class="system-monitor__header">
      <h1>系统监控</h1>
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
        <a-button type="primary" :loading="loading" @click="fetchDeviceInfo"> 刷新 </a-button>
      </a-space>
    </div>

    <a-spin :spinning="loading">
      <a-alert v-if="error" type="error" :message="error" show-icon style="margin-bottom: 16px" />

      <div v-if="deviceInfo">
        <!-- 关键指标卡片区 -->
        <a-row :gutter="16" class="metrics-cards">
          <!-- CPU使用率卡片 -->
          <a-col :xs="24" :sm="12" :lg="6">
            <a-card>
              <a-statistic
                title="CPU使用率"
                :value="deviceInfo.cpuUsage.toFixed(1)"
                suffix="%"
                :value-style="{ color: getCpuColor(deviceInfo.cpuUsage) }"
              />
              <a-progress
                :percent="Number(deviceInfo.cpuUsage.toFixed(1))"
                :stroke-color="getCpuColor(deviceInfo.cpuUsage)"
                :show-info="false"
                style="margin-top: 8px"
              />
              <div class="card-footer">
                <a-tag color="blue">{{ deviceInfo.processorCount }} 核心</a-tag>
              </div>
            </a-card>
          </a-col>

          <!-- 内存使用率卡片 -->
          <a-col :xs="24" :sm="12" :lg="6">
            <a-card>
              <a-statistic
                title="内存使用率"
                :value="memoryUsagePercent"
                suffix="%"
                :value-style="{ color: getMemoryColor(Number(memoryUsagePercent)) }"
              />
              <a-progress
                :percent="Number(memoryUsagePercent)"
                :stroke-color="getMemoryColor(Number(memoryUsagePercent))"
                style="margin-top: 8px"
              />
              <div class="card-footer">{{ usedMemoryGB }} GB / {{ totalMemoryGB }} GB</div>
            </a-card>
          </a-col>

          <!-- 系统运行时间卡片 -->
          <a-col :xs="24" :sm="12" :lg="6">
            <a-card>
              <a-statistic title="系统运行时间" :value="formattedUptime">
                <template #suffix>
                  <a-tag color="green" style="margin-left: 8px">在线</a-tag>
                </template>
              </a-statistic>
              <div class="card-footer" style="margin-top: 24px">
                <small>{{ deviceInfo.machineName }}</small>
              </div>
            </a-card>
          </a-col>

          <!-- 进程信息卡片 -->
          <a-col :xs="24" :sm="12" :lg="6">
            <a-card>
              <a-statistic
                title="进程内存"
                :value="deviceInfo.processMemoryMB"
                suffix="MB"
                :value-style="{ color: '#1890ff' }"
              />
              <div class="card-footer" style="margin-top: 24px">
                <a-tag color="purple">{{ deviceInfo.processArchitecture }}</a-tag>
                <a-tag :color="deviceInfo.is64BitProcess ? 'green' : 'orange'">
                  {{ deviceInfo.is64BitProcess ? '64位' : '32位' }}
                </a-tag>
              </div>
            </a-card>
          </a-col>
        </a-row>

        <!-- 详细信息Tab区域 -->
        <a-tabs default-active-key="trend" class="detail-tabs">
          <!-- 性能趋势Tab -->
          <a-tab-pane key="trend" tab="性能趋势">
            <a-row :gutter="16">
              <a-col :xs="24" :lg="12">
                <a-card title="CPU使用率趋势" :bordered="false">
                  <v-chart
                    v-if="cpuHistory.length > 0"
                    :option="cpuChartOption"
                    :autoresize="true"
                    class="chart"
                  />
                  <a-empty v-else description="暂无历史数据，请等待数据采集" />
                </a-card>
              </a-col>
              <a-col :xs="24" :lg="12">
                <a-card title="内存使用率趋势" :bordered="false">
                  <v-chart
                    v-if="memoryHistory.length > 0"
                    :option="memoryChartOption"
                    :autoresize="true"
                    class="chart"
                  />
                  <a-empty v-else description="暂无历史数据，请等待数据采集" />
                </a-card>
              </a-col>
            </a-row>
          </a-tab-pane>

          <!-- 系统信息Tab -->
          <a-tab-pane key="system" tab="系统信息">
            <a-descriptions bordered :column="2">
              <a-descriptions-item label="操作系统">
                {{ deviceInfo.os }}
              </a-descriptions-item>
              <a-descriptions-item label="系统版本">
                {{ deviceInfo.osVersion }}
              </a-descriptions-item>
              <a-descriptions-item label="机器名称">
                {{ deviceInfo.machineName }}
              </a-descriptions-item>
              <a-descriptions-item label="系统架构">
                {{ deviceInfo.architecture }}
              </a-descriptions-item>
              <a-descriptions-item label="处理器数量">
                {{ deviceInfo.processorCount }} 核
              </a-descriptions-item>
              <a-descriptions-item label="64位操作系统">
                <a-tag :color="deviceInfo.is64BitOS ? 'green' : 'red'">
                  {{ deviceInfo.is64BitOS ? '是' : '否' }}
                </a-tag>
              </a-descriptions-item>
              <a-descriptions-item label="总内存">
                {{ (deviceInfo.totalMemoryMB / 1024).toFixed(2) }} GB
              </a-descriptions-item>
              <a-descriptions-item label="可用内存">
                {{ (deviceInfo.availableMemoryMB / 1024).toFixed(2) }} GB
              </a-descriptions-item>
              <a-descriptions-item label="系统目录" :span="2">
                {{ deviceInfo.systemDirectory }}
              </a-descriptions-item>
              <a-descriptions-item label="当前目录" :span="2">
                {{ deviceInfo.currentDirectory }}
              </a-descriptions-item>
            </a-descriptions>
          </a-tab-pane>

          <!-- 环境信息Tab -->
          <a-tab-pane key="env" tab="环境信息">
            <a-descriptions bordered :column="2">
              <a-descriptions-item label=".NET 版本">
                {{ deviceInfo.dotNetVersion }}
              </a-descriptions-item>
              <a-descriptions-item label="进程架构">
                {{ deviceInfo.processArchitecture }}
              </a-descriptions-item>
              <a-descriptions-item label="64位进程">
                <a-tag :color="deviceInfo.is64BitProcess ? 'green' : 'red'">
                  {{ deviceInfo.is64BitProcess ? '是' : '否' }}
                </a-tag>
              </a-descriptions-item>
              <a-descriptions-item label="进程内存">
                {{ deviceInfo.processMemoryMB }} MB
              </a-descriptions-item>
              <a-descriptions-item label="系统运行时间">
                {{ formatUptime(deviceInfo.tickCount) }}
              </a-descriptions-item>
            </a-descriptions>
          </a-tab-pane>
        </a-tabs>
      </div>
    </a-spin>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { use } from 'echarts/core'
import { CanvasRenderer } from 'echarts/renderers'
import { LineChart } from 'echarts/charts'
import {
  TitleComponent,
  TooltipComponent,
  GridComponent,
  LegendComponent,
} from 'echarts/components'
import VChart from 'vue-echarts'
import { useSystemMonitor } from '@/composables/useSystemMonitor'

// 注册ECharts组件
use([CanvasRenderer, LineChart, TitleComponent, TooltipComponent, GridComponent, LegendComponent])

// 使用composable
const {
  deviceInfo,
  loading,
  error,
  historyData,
  fetchDeviceInfo,
  startAutoRefresh,
  stopAutoRefresh,
  setRefreshInterval,
} = useSystemMonitor()

// 自动刷新控制
const autoRefreshEnabled = ref(false)
const refreshIntervalValue = ref(10000)

const intervalOptions = [
  { label: '5秒', value: 5000 },
  { label: '10秒', value: 10000 },
  { label: '30秒', value: 30000 },
  { label: '60秒', value: 60000 },
]

// 计算属性
const memoryUsagePercent = computed(() => {
  if (!deviceInfo.value) return '0.0'
  const used = deviceInfo.value.totalMemoryMB - deviceInfo.value.availableMemoryMB
  return ((used / deviceInfo.value.totalMemoryMB) * 100).toFixed(1)
})

const usedMemoryGB = computed(() => {
  if (!deviceInfo.value) return '0.00'
  const used = deviceInfo.value.totalMemoryMB - deviceInfo.value.availableMemoryMB
  return (used / 1024).toFixed(2)
})

const totalMemoryGB = computed(() => {
  if (!deviceInfo.value) return '0.00'
  return (deviceInfo.value.totalMemoryMB / 1024).toFixed(2)
})

const formattedUptime = computed(() => {
  if (!deviceInfo.value) return '0天'
  return formatUptime(deviceInfo.value.tickCount)
})

const cpuHistory = computed(() => {
  return historyData.value.map((point) => point.cpuUsage)
})

const memoryHistory = computed(() => {
  return historyData.value.map((point) => point.memoryUsage)
})

// ECharts配置
const cpuChartOption = computed(() => ({
  tooltip: {
    trigger: 'axis',
    formatter: '{b}<br/>CPU使用率: {c}%',
  },
  grid: {
    left: '3%',
    right: '4%',
    bottom: '3%',
    top: '10%',
  },
  xAxis: {
    type: 'category',
    boundaryGap: false,
    show: false,
  },
  yAxis: {
    type: 'value',
    min: 0,
    max: 100,
    axisLabel: {
      formatter: '{value}%',
    },
  },
  series: [
    {
      name: 'CPU使用率',
      type: 'line',
      smooth: true,
      symbol: 'none',
      sampling: 'lttb',
      itemStyle: {
        color: '#1890ff',
      },
      areaStyle: {
        color: {
          type: 'linear',
          x: 0,
          y: 0,
          x2: 0,
          y2: 1,
          colorStops: [
            { offset: 0, color: 'rgba(24, 144, 255, 0.3)' },
            { offset: 1, color: 'rgba(24, 144, 255, 0.05)' },
          ],
        },
      },
      data: cpuHistory.value,
    },
  ],
}))

const memoryChartOption = computed(() => ({
  tooltip: {
    trigger: 'axis',
    formatter: '{b}<br/>内存使用率: {c}%',
  },
  grid: {
    left: '3%',
    right: '4%',
    bottom: '3%',
    top: '10%',
  },
  xAxis: {
    type: 'category',
    boundaryGap: false,
    show: false,
  },
  yAxis: {
    type: 'value',
    min: 0,
    max: 100,
    axisLabel: {
      formatter: '{value}%',
    },
  },
  series: [
    {
      name: '内存使用率',
      type: 'line',
      smooth: true,
      symbol: 'none',
      sampling: 'lttb',
      itemStyle: {
        color: '#52c41a',
      },
      areaStyle: {
        color: {
          type: 'linear',
          x: 0,
          y: 0,
          x2: 0,
          y2: 1,
          colorStops: [
            { offset: 0, color: 'rgba(82, 196, 26, 0.3)' },
            { offset: 1, color: 'rgba(82, 196, 26, 0.05)' },
          ],
        },
      },
      data: memoryHistory.value,
    },
  ],
}))

// 辅助方法
function getCpuColor(usage: number): string {
  if (usage < 60) return '#52c41a' // 绿色
  if (usage < 85) return '#faad14' // 橙色
  return '#f5222d' // 红色
}

function getMemoryColor(usage: number): string {
  if (usage < 70) return '#52c41a' // 绿色
  if (usage < 90) return '#faad14' // 橙色
  return '#f5222d' // 红色
}

function formatUptime(milliseconds: number): string {
  const seconds = Math.floor(milliseconds / 1000)
  const days = Math.floor(seconds / 86400)
  const hours = Math.floor((seconds % 86400) / 3600)
  const minutes = Math.floor((seconds % 3600) / 60)
  return `${days}天 ${hours}小时 ${minutes}分钟`
}

function handleAutoRefreshToggle(checked: boolean) {
  if (checked) {
    startAutoRefresh()
  } else {
    stopAutoRefresh()
  }
}

function handleIntervalChange(value: number) {
  setRefreshInterval(value)
}

// 生命周期
onMounted(() => {
  fetchDeviceInfo()
})

onUnmounted(() => {
  stopAutoRefresh()
})
</script>

<style scoped>
.system-monitor {
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 24px;
  background: #f0f2f5;
  min-height: calc(100vh - 64px);
}

.system-monitor__header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: white;
  padding: 16px 24px;
  border-radius: 8px;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.03);
}

.system-monitor__header h1 {
  margin: 0;
  font-size: 24px;
  font-weight: 600;
  color: rgba(0, 0, 0, 0.85);
}

/* 关键指标卡片区 */
.metrics-cards {
  margin-bottom: 16px;
}

.metrics-cards :deep(.ant-card) {
  border-radius: 8px;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.03);
  transition: all 0.3s ease;
  height: 100%;
}

.metrics-cards :deep(.ant-card:hover) {
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  transform: translateY(-2px);
}

.metrics-cards :deep(.ant-card-body) {
  padding: 20px;
}

.metrics-cards :deep(.ant-statistic-title) {
  font-size: 14px;
  color: rgba(0, 0, 0, 0.45);
  margin-bottom: 8px;
}

.metrics-cards :deep(.ant-statistic-content) {
  font-size: 24px;
  font-weight: 600;
}

.card-footer {
  margin-top: 12px;
  font-size: 12px;
  color: rgba(0, 0, 0, 0.45);
}

/* Tab区域 */
.detail-tabs {
  background: white;
  padding: 16px;
  border-radius: 8px;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.03);
}

.detail-tabs :deep(.ant-tabs-nav) {
  margin-bottom: 16px;
}

.detail-tabs :deep(.ant-card) {
  border-radius: 8px;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.03);
}

.detail-tabs :deep(.ant-descriptions) {
  background: white;
}

/* ECharts图表容器 */
.chart {
  width: 100%;
  height: 250px;
}

/* 响应式调整 */
@media (max-width: 768px) {
  .system-monitor {
    padding: 16px;
    gap: 16px;
  }

  .system-monitor__header {
    flex-direction: column;
    gap: 16px;
    align-items: flex-start;
  }

  .system-monitor__header h1 {
    font-size: 20px;
  }

  .metrics-cards :deep(.ant-col) {
    margin-bottom: 16px;
  }

  .chart {
    height: 200px;
  }
}

/* 加载动画 */
:deep(.ant-spin-container) {
  transition: opacity 0.3s;
}

/* 进度条动画 */
:deep(.ant-progress-bg) {
  transition: all 0.3s cubic-bezier(0.78, 0.14, 0.15, 0.86);
}

/* Tag样式优化 */
:deep(.ant-tag) {
  border-radius: 4px;
  padding: 2px 8px;
  font-size: 12px;
}
</style>
