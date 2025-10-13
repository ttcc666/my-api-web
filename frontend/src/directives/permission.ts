import type { App, Directive, DirectiveBinding } from 'vue'
import { useAuthStore } from '@/stores/auth'

function checkPermission(el: HTMLElement, binding: DirectiveBinding<string | string[]>) {
  const authStore = useAuthStore()
  const { value } = binding

  if (!value) {
    throw new Error("v-permission requires a value, e.g., v-permission=\"'manage:users'\" or v-permission=\"['view:dashboard', 'edit:articles']\"")
  }

  let hasPermission = false
  if (typeof value === 'string') {
    hasPermission = authStore.hasPermission(value)
  } else if (Array.isArray(value)) {
    // 默认检查是否拥有其中任一权限 (OR)
    // 如果需要检查是否拥有所有权限 (AND)，可以使用修饰符 .all
    const isAll = binding.modifiers.all
    hasPermission = isAll ? authStore.hasAllPermissions(value) : authStore.hasAnyPermission(value)
  }

  if (!hasPermission) {
    // 如果没有权限，从DOM中移除该元素
    if (el.parentNode) {
      el.parentNode.removeChild(el)
    }
  }
}

const permissionDirective: Directive = {
  mounted(el, binding) {
    checkPermission(el, binding)
  },
  updated(el, binding) {
    checkPermission(el, binding)
  }
}

export function setupPermissionDirective(app: App) {
  app.directive('permission', permissionDirective)
}

export default permissionDirective