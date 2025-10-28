/**
 * 通用 API 响应类型模块
 * Common API Response Types Module
 */

/**
 * API响应的通用结构
 */
export interface ApiResponse<T = unknown> {
  success: boolean
  code: number
  message: string
  data: T | null
  total?: number // 用于分页
}

/**
 * API错误详情
 */
export interface ApiError {
  message: string
  status?: number
}

/**
 * 检查资源是否存在响应的DTO
 */
export interface ExistsResponseDto {
  exists: boolean
}

// 导出验证码相关类型
export type { CaptchaResponse, CaptchaValidateRequest, CaptchaValidateResponse } from './captcha'
