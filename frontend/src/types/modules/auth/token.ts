/**
 * Token 相关类型定义
 * Token Related Type Definitions
 */

/**
 * Token响应DTO
 */
export interface TokenDto {
  accessToken: string
  refreshToken: string
}

/**
 * 刷新Token请求DTO
 */
export interface RefreshTokenRequestDto {
  refreshToken: string
}

/**
 * @deprecated 使用 TokenDto 替代
 */
export type TokenPayload = TokenDto
