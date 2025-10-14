import apiClient from '@/utils/request'
import type { TokenDto, RefreshTokenRequestDto } from '@/types/api'

/**
 * Token 相关 API 服务
 */
export class TokenApi {
  /**
   * 刷新访问令牌
   */
  static async refreshToken(refreshTokenData: RefreshTokenRequestDto): Promise<TokenDto> {
    return apiClient.post('/token/refresh', refreshTokenData)
  }

  /**
   * 用户登出（撤销刷新令牌）
   */
  static async logout(refreshTokenData: RefreshTokenRequestDto): Promise<void> {
    return apiClient.post('/token/logout', refreshTokenData)
  }
}

export default TokenApi
