import { message, notification, Modal } from 'ant-design-vue'

message.config({
  maxCount: 3,
  duration: 3,
})

notification.config({
  placement: 'topRight',
  duration: 3,
})

const modal = Modal

export { message, notification, modal }
