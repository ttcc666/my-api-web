import apiClient from '@/utils/request'
import type { MenuDto, CreateMenuDto, UpdateMenuDto } from '@/types/api'

/**
 * 菜单管理 API 服务
 */
export class MenusApi {
  /**
   * 获取菜单列表
   */
  static async getMenus(params?: { onlyEnabled?: boolean }): Promise<MenuDto[]> {
    return apiClient.get('/menus', { params })
  }

  /**
   * 获取菜单树
   */
  static async getMenuTree(params?: { onlyEnabled?: boolean }): Promise<MenuDto[]> {
    return apiClient.get('/menus/tree', { params })
  }

  /**
   * 根据 ID 获取菜单
   */
  static async getMenuById(id: string): Promise<MenuDto> {
    return apiClient.get(`/menus/${id}`)
  }

  /**
   * 创建菜单
   */
  static async createMenu(payload: CreateMenuDto): Promise<MenuDto> {
    return apiClient.post('/menus', payload)
  }

  /**
   * 更新菜单
   */
  static async updateMenu(id: string, payload: UpdateMenuDto): Promise<MenuDto> {
    return apiClient.put(`/menus/${id}`, payload)
  }

  /**
   * 删除菜单
   */
  static async deleteMenu(id: string): Promise<void> {
    await apiClient.delete(`/menus/${id}`)
  }
}

export default MenusApi
