/**
 * 在线用户相关类型定义
 * Online User Related Type Definitions
 */

/**
 * 在线用户状态枚举
 */
export enum OnlineUserStatus {
  Online = 'Online',
  Idle = 'Idle',
  Offline = 'Offline',
}

/**
 * 在线用户信息 DTO
 */
export interface OnlineUserDto {
  id: string
  connectionId: string
  userId: string
  username?: string | null
  connectedAt: string
  lastHeartbeatAt: string
  ipAddress?: string | null
  userAgent?: string | null
  room?: string | null
  status: OnlineUserStatus
  disconnectedAt?: string | null
  onlineDurationSeconds?: number
}

/**
 * 在线用户统计信息 DTO
 */
export interface OnlineUserStatisticsDto {
  totalOnlineUsers: number
  totalConnections: number
  todayPeakUsers: number
  activeUsers: number
  idleUsers: number
  averageOnlineDurationSeconds: number
  statisticsTime: string
}

/**
 * 在线用户查询参数
 */
export interface OnlineUserQueryParams {
  pageNumber?: number
  pageSize?: number
  status?: OnlineUserStatus
  userId?: string
  room?: string
}

/**
 * 在线用户列表响应 DTO
 */
export interface OnlineUserListResponseDto {
  users: OnlineUserDto[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
}

/**
 * 在线用户数量响应 DTO
 */
export interface OnlineUserCountResponseDto {
  count: number
}
