import {
  createDiscreteApi,
  type ConfigProviderProps
} from 'naive-ui'
import { computed } from 'vue'

const configProviderPropsRef = computed<ConfigProviderProps>(() => ({
  // 你可以在这里自定义 Naive UI 的主题
  // theme: darkTheme
}))

const { message, notification, dialog, loadingBar } = createDiscreteApi(
  ['message', 'dialog', 'notification', 'loadingBar'],
  {
    configProviderProps: configProviderPropsRef
  }
)

export { message, notification, dialog, loadingBar }