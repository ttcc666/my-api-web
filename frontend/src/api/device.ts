import apiClient from '@/utils/request'

export interface DeviceInfoDto {
  os: string
  osVersion: string
  machineName: string
  processorCount: number
  tickCount: number
  dotNetVersion: string
  architecture: string
  processArchitecture: string
  totalMemoryMB: number
  availableMemoryMB: number
  processMemoryMB: number
  cpuUsage: number
  systemDirectory: string
  currentDirectory: string
  is64BitOS: boolean
  is64BitProcess: boolean
}

export class DeviceApi {
  static async getDeviceInfo(): Promise<DeviceInfoDto> {
    return apiClient.get('/device/info')
  }
}

export default DeviceApi
