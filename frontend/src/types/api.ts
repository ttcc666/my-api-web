/**
 * API 类型定义 - 兼容层
 * API Type Definitions - Compatibility Layer
 *
 * 此文件现在作为兼容层,重新导出所有模块化的类型定义
 * This file now serves as a compatibility layer, re-exporting all modularized type definitions
 *
 * 推荐使用方式 (Recommended Usage):
 * - 新代码建议直接从模块导入: import { UserDto } from '@/types/modules/system/user'
 * - New code should import directly from modules: import { UserDto } from '@/types/modules/system/user'
 *
 * 向后兼容方式 (Backward Compatibility):
 * - 现有代码可继续使用: import { UserDto } from '@/types/api'
 * - Existing code can continue to use: import { UserDto } from '@/types/api'
 */

// ===================================
// 重新导出所有模块类型
// Re-export all module types
// ===================================

export * from './modules/common'
export * from './modules/system'
export * from './modules/auth'
export * from './modules/hub'
