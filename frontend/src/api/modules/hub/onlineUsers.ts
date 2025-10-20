import apiClient from '@/utils/request'
import type {
  OnlineUserDto,
  OnlineUserListResponseDto,
  OnlineUserStatisticsDto,
  OnlineUserQueryParams,
  OnlineUserCountResponseDto,
} from '@/types/api'

/**
 * 在线用户 API 服务
 */
export class OnlineUsersApi {
  /**
   * 查询在线用户列表
   */
  static async getOnlineUsers(params?: OnlineUserQueryParams): Promise<OnlineUserListResponseDto> {
    return apiClient.get('/online-users', { params })
  }

  /**
   * 获取在线用户统计信息
   */
  static async getStatistics(): Promise<OnlineUserStatisticsDto> {
    return apiClient.get('/online-users/statistics')
  }

  /**
   * 查询指定用户的所有连接
   */
  static async getUserConnections(userId: string): Promise<OnlineUserDto[]> {
    return apiClient.get(`/online-users/by-user/${userId}`)
  }

  /**
   * 获取在线用户数量
   */
  static async getCount(): Promise<OnlineUserCountResponseDto> {
    return apiClient.get('/online-users/count')
  }

  /**
   * 强制下线指定连接
   */
  static async forceDisconnect(connectionId: string, reason?: string): Promise<void> {
    return apiClient.delete(`/online-users/${connectionId}`, {
      params: { reason },
    })
  }

  /**
   * 手动清理超时连接
   */
  static async cleanup(timeoutMinutes?: number): Promise<{ cleanedCount: number }> {
    return apiClient.post('/online-users/cleanup', null, {
      params: { timeoutMinutes },
    })
  }
}

export default OnlineUsersApi
