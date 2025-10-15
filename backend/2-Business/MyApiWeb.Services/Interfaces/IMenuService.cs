using MyApiWeb.Models.DTOs;

namespace MyApiWeb.Services.Interfaces
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
