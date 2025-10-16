<template>
  <div class="system-monitor">
    <div class="system-monitor__header">
      <h1>系统监控</h1>
      <a-button type="primary" :loading="loading" @click="fetchDeviceInfo"> 刷新 </a-button>
    </div>

    <a-spin :spinning="loading">
      <a-alert v-if="error" type="error" :message="error" show-icon style="margin-bottom: 16px" />

      <a-descriptions v-if="deviceInfo" bordered :column="2">
        <a-descriptions-item label="操作系统">
          {{ deviceInfo.os }}
        </a-descriptions-item>
        <a-descriptions-item label="系统版本">
          {{ deviceInfo.osVersion }}
        </a-descriptions-item>
        <a-descriptions-item label="机器名称">
          {{ deviceInfo.machineName }}
        </a-descriptions-item>
        <a-descriptions-item label=".NET 版本">
          {{ deviceInfo.dotNetVersion }}
        </a-descriptions-item>
        <a-descriptions-item label="处理器数量">
          {{ deviceInfo.processorCount }} 核
        </a-descriptions-item>
        <a-descriptions-item label="CPU 使用率">
          {{ deviceInfo.cpuUsage.toFixed(2) }}%
        </a-descriptions-item>
        <a-descriptions-item label="系统架构">
          {{ deviceInfo.architecture }}
        </a-descriptions-item>
        <a-descriptions-item label="进程架构">
          {{ deviceInfo.processArchitecture }}
        </a-descriptions-item>
        <a-descriptions-item label="总内存">
          {{ (deviceInfo.totalMemoryMB / 1024).toFixed(2) }} GB
        </a-descriptions-item>
        <a-descriptions-item label="可用内存">
          {{ (deviceInfo.availableMemoryMB / 1024).toFixed(2) }} GB
        </a-descriptions-item>
        <a-descriptions-item label="进程内存">
          {{ deviceInfo.processMemoryMB }} MB
        </a-descriptions-item>
        <a-descriptions-item label="系统运行时间">
          {{ formatUptime(deviceInfo.tickCount) }}
        </a-descriptions-item>
        <a-descriptions-item label="64位操作系统">
          <a-tag :color="deviceInfo.is64BitOS ? 'green' : 'red'">
            {{ deviceInfo.is64BitOS ? '是' : '否' }}
          </a-tag>
        </a-descriptions-item>
        <a-descriptions-item label="64位进程">
          <a-tag :color="deviceInfo.is64BitProcess ? 'green' : 'red'">
            {{ deviceInfo.is64BitProcess ? '是' : '否' }}
          </a-tag>
        </a-descriptions-item>
        <a-descriptions-item label="系统目录" :span="2">
          {{ deviceInfo.systemDirectory }}
        </a-descriptions-item>
        <a-descriptions-item label="当前目录" :span="2">
          {{ deviceInfo.currentDirectory }}
        </a-descriptions-item>
      </a-descriptions>
    </a-spin>
  </div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import { useSystemMonitor } from '@/composables/useSystemMonitor'

const { deviceInfo, loading, error, fetchDeviceInfo } = useSystemMonitor()

function formatUptime(milliseconds: number): string {
  const seconds = Math.floor(milliseconds / 1000)
  const days = Math.floor(seconds / 86400)
  const hours = Math.floor((seconds % 86400) / 3600)
  const minutes = Math.floor((seconds % 3600) / 60)
  return `${days} 天 ${hours} 小时 ${minutes} 分钟`
}

onMounted(() => {
  fetchDeviceInfo()
})
</script>

<style scoped>
.system-monitor {
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.system-monitor__header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.system-monitor__header h1 {
  margin: 0;
  font-size: 24px;
  font-weight: 600;
}
</style>
