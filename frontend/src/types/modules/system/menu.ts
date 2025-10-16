/**
 * 菜单相关类型定义
 * Menu Related Type Definitions
 */

/**
 * 菜单类型
 */
export enum MenuType {
  Directory = 0,
  Route = 1,
}

/**
 * 菜单信息 DTO
 */
export interface MenuDto {
  id: string
  code: string
  title: string
  routePath?: string | null
  routeName?: string | null
  icon?: string | null
  parentId?: string | null
  order: number
  isEnabled: boolean
  type: MenuType
  permissionCode?: string | null
  description?: string | null
  children?: MenuDto[]
}

/**
 * 创建菜单 DTO
 */
export interface CreateMenuDto {
  code: string
  title: string
  routePath?: string | null
  routeName?: string | null
  icon?: string | null
  parentId?: string | null
  order: number
  isEnabled: boolean
  type: MenuType
  permissionCode?: string | null
  description?: string | null
}

/**
 * 更新菜单 DTO
 */
export interface UpdateMenuDto {
  title: string
  routePath?: string | null
  routeName?: string | null
  icon?: string | null
  parentId?: string | null
  order: number
  isEnabled: boolean
  type: MenuType
  permissionCode?: string | null
  description?: string | null
}
