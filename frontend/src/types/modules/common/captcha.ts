// 验证码相关类型定义
export interface CaptchaResponse {
  captchaId: string
  image: string
  expiryMinutes: number
}

export interface CaptchaValidateRequest {
  captchaId: string
  code: string
}

export interface CaptchaValidateResponse {
  isValid: boolean
}