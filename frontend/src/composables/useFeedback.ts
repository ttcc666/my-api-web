import { message, notification, modal } from '@/plugins/antd'
import type { ModalFuncProps } from 'ant-design-vue'

export interface NotifyPayload {
  title: string
  description?: string
  duration?: number
}

export function useFeedback() {
  const success = (content: string, duration?: number) => message.success({ content, duration })
  const error = (content: string, duration?: number) => message.error({ content, duration })
  const info = (content: string, duration?: number) => message.info({ content, duration })
  const warning = (content: string, duration?: number) => message.warning({ content, duration })

  const notifySuccess = ({ title, description, duration }: NotifyPayload) =>
    notification.success({
      message: title,
      description,
      duration,
    })

  const notifyError = ({ title, description, duration }: NotifyPayload) =>
    notification.error({
      message: title,
      description,
      duration,
    })

  const confirm = (props: ModalFuncProps) => modal.confirm(props)

  return {
    message: {
      success,
      error,
      info,
      warning,
    },
    notification: {
      success: notifySuccess,
      error: notifyError,
    },
    confirm,
  }
}
