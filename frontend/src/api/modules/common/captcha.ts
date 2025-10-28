import apiClient from '@/utils/request'
import type { CaptchaResponse, CaptchaValidateRequest } from '@/types/modules/common'

/**
 * 验证码 API
 */
export class CaptchaApi {
  /**
   * 生成验证码
   */
  static async generateCaptcha(): Promise<CaptchaResponse> {
    return apiClient.get('/captcha/generate')
  }

  /**
   * 验证验证码
   */
  static async validateCaptcha(request: CaptchaValidateRequest): Promise<{ isValid: boolean }> {
    return apiClient.post('/captcha/validate', request)
  }
}

// 导出静态方法的引用
export const captchaApi = CaptchaApi