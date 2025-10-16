using MyApiWeb.Models.DTOs;
using MyApiWeb.Models.Entities.Common;
using MyApiWeb.Models.Entities.System;
using MyApiWeb.Models.Entities.Auth;
using MyApiWeb.Models.Entities.Hub;

namespace MyApiWeb.Services.Interfaces.System
{
    /// <summary>
    /// 菜单服务接口
    /// </summary>
    public interface IMenuService
    {
        Task<List<MenuDto>> GetAllMenusAsync(bool onlyEnabled = false);

        Task<List<MenuDto>> GetMenuTreeAsync(bool onlyEnabled = false);

        Task<MenuDto?> GetMenuByIdAsync(string id);

        Task<MenuDto> CreateMenuAsync(CreateMenuDto createMenuDto);

        Task<MenuDto> UpdateMenuAsync(string id, UpdateMenuDto updateMenuDto);

        Task<bool> DeleteMenuAsync(string id);

        Task<List<MenuDto>> GetMenusByUserAsync(string userId);
    }
}
