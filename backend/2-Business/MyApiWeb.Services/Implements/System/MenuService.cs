using MyApiWeb.Models.DTOs;
using MyApiWeb.Models.Entities.Common;
using MyApiWeb.Models.Entities.System;
using MyApiWeb.Models.Entities.Auth;
using MyApiWeb.Models.Entities.Hub;
using MyApiWeb.Models.Exceptions;
using MyApiWeb.Repository;
using MyApiWeb.Services.Interfaces.System;
using SqlSugar;

namespace MyApiWeb.Services.Implements.System
{
    /// <summary>
    /// 菜单服务实现
    /// </summary>
    public class MenuService : IMenuService
    {
        private readonly SqlSugarDbContext _dbContext;
        private readonly IPermissionService _permissionService;

        public MenuService(SqlSugarDbContext dbContext, IPermissionService permissionService)
        {
            _dbContext = dbContext;
            _permissionService = permissionService;
        }

        public async Task<List<MenuDto>> GetAllMenusAsync(bool onlyEnabled = false)
        {
            var query = _dbContext.Queryable<Menu>().OrderBy(m => m.Order);

            if (onlyEnabled)
            {
                query = query.Where(m => m.IsEnabled);
            }

            var menus = await query.ToListAsync();
            return menus.Select(MapToDto).ToList();
        }

        public async Task<List<MenuDto>> GetMenuTreeAsync(bool onlyEnabled = false)
        {
            var query = _dbContext.Queryable<Menu>();

            if (onlyEnabled)
            {
                query = query.Where(m => m.IsEnabled);
            }

            var menus = await query.ToListAsync();
            var tree = BuildMenuTree(menus);
            SortMenuTree(tree);
            return tree;
        }

        public async Task<MenuDto?> GetMenuByIdAsync(string id)
        {
            var menu = await _dbContext.Queryable<Menu>()
                .Where(m => m.Id == id)
                .FirstAsync();

            return menu == null ? null : MapToDto(menu);
        }

        public async Task<MenuDto> CreateMenuAsync(CreateMenuDto createMenuDto)
        {
            if (await MenuCodeExistsAsync(createMenuDto.Code))
            {
                throw new BusinessException($"菜单编码 '{createMenuDto.Code}' 已存在");
            }

            if (!string.IsNullOrEmpty(createMenuDto.ParentId))
            {
                var parentExists = await _dbContext.Queryable<Menu>()
                    .Where(m => m.Id == createMenuDto.ParentId)
                    .AnyAsync();

                if (!parentExists)
                {
                    throw new BusinessException("上级菜单不存在");
                }
            }

            var menu = new Menu
            {
                Id = Guid.NewGuid().ToString(),
                Code = createMenuDto.Code,
                Title = createMenuDto.Title,
                RoutePath = string.IsNullOrWhiteSpace(createMenuDto.RoutePath) ? null : createMenuDto.RoutePath,
                RouteName = string.IsNullOrWhiteSpace(createMenuDto.RouteName) ? null : createMenuDto.RouteName,
                Icon = string.IsNullOrWhiteSpace(createMenuDto.Icon) ? null : createMenuDto.Icon,
                ParentId = string.IsNullOrWhiteSpace(createMenuDto.ParentId) ? null : createMenuDto.ParentId,
                PermissionCode = string.IsNullOrWhiteSpace(createMenuDto.PermissionCode) ? null : createMenuDto.PermissionCode,
                Description = string.IsNullOrWhiteSpace(createMenuDto.Description) ? null : createMenuDto.Description,
                Order = createMenuDto.Order,
                Type = createMenuDto.Type,
                IsEnabled = createMenuDto.IsEnabled
            };

            await _dbContext.Insertable(menu).ExecuteCommandAsync();

            return MapToDto(menu);
        }

        public async Task<MenuDto> UpdateMenuAsync(string id, UpdateMenuDto updateMenuDto)
        {
            var menu = await _dbContext.Queryable<Menu>()
                .Where(m => m.Id == id)
                .FirstAsync();

            if (menu == null)
            {
                throw new BusinessException("菜单不存在");
            }

            if (!string.IsNullOrEmpty(updateMenuDto.ParentId) && updateMenuDto.ParentId == id)
            {
                throw new BusinessException("菜单不能设置为自己的父级");
            }

            if (!string.IsNullOrEmpty(updateMenuDto.ParentId))
            {
                var parentExists = await _dbContext.Queryable<Menu>()
                    .Where(m => m.Id == updateMenuDto.ParentId)
                    .AnyAsync();

                if (!parentExists)
                {
                    throw new BusinessException("上级菜单不存在");
                }
            }

            menu.Title = updateMenuDto.Title;
            menu.RoutePath = string.IsNullOrWhiteSpace(updateMenuDto.RoutePath) ? null : updateMenuDto.RoutePath;
            menu.RouteName = string.IsNullOrWhiteSpace(updateMenuDto.RouteName) ? null : updateMenuDto.RouteName;
            menu.Icon = string.IsNullOrWhiteSpace(updateMenuDto.Icon) ? null : updateMenuDto.Icon;
            menu.ParentId = string.IsNullOrWhiteSpace(updateMenuDto.ParentId) ? null : updateMenuDto.ParentId;
            menu.PermissionCode = string.IsNullOrWhiteSpace(updateMenuDto.PermissionCode) ? null : updateMenuDto.PermissionCode;
            menu.Description = string.IsNullOrWhiteSpace(updateMenuDto.Description) ? null : updateMenuDto.Description;
            menu.Order = updateMenuDto.Order;
            menu.Type = updateMenuDto.Type;
            menu.IsEnabled = updateMenuDto.IsEnabled;

            await _dbContext.Updateable(menu).ExecuteCommandAsync();

            return MapToDto(menu);
        }

        public async Task<bool> DeleteMenuAsync(string id)
        {
            var menu = await _dbContext.Queryable<Menu>()
                .Where(m => m.Id == id)
                .FirstAsync();

            if (menu == null)
            {
                throw new BusinessException("菜单不存在");
            }

            var hasChildren = await _dbContext.Queryable<Menu>()
                .Where(m => m.ParentId == id)
                .AnyAsync();

            if (hasChildren)
            {
                throw new BusinessException("请先删除子菜单");
            }

            var result = await _dbContext.Deleteable<Menu>()
                .Where(m => m.Id == id)
                .ExecuteCommandAsync();

            return result > 0;
        }

        public async Task<List<MenuDto>> GetMenusByUserAsync(string userId)
        {
            var permissionInfo = await _permissionService.GetUserPermissionsAsync(userId);
            var permissionCodes = new HashSet<string>(permissionInfo.EffectivePermissions.Select(p => p.Name));

            var menus = await _dbContext.Queryable<Menu>()
                .Where(m => m.IsEnabled)
                .OrderBy(m => m.Order)
                .ToListAsync();

            var filtered = menus.Where(menu =>
                    menu.Type == MenuType.Directory ||
                    string.IsNullOrEmpty(menu.PermissionCode) ||
                    permissionCodes.Contains(menu.PermissionCode))
                .ToList();

            var tree = BuildMenuTree(filtered);
            PruneEmptyDirectories(tree);
            SortMenuTree(tree);

            return tree;
        }

        private async Task<bool> MenuCodeExistsAsync(string code)
        {
            return await _dbContext.Queryable<Menu>()
                .Where(m => m.Code == code)
                .AnyAsync();
        }

        private static MenuDto MapToDto(Menu menu)
        {
            return new MenuDto
            {
                Id = menu.Id,
                Code = menu.Code,
                Title = menu.Title,
                RoutePath = menu.RoutePath,
                RouteName = menu.RouteName,
                Icon = menu.Icon,
                ParentId = menu.ParentId,
                Order = menu.Order,
                IsEnabled = menu.IsEnabled,
                Type = menu.Type,
                PermissionCode = menu.PermissionCode,
                Description = menu.Description,
                Children = new List<MenuDto>()
            };
        }

        private static List<MenuDto> BuildMenuTree(IEnumerable<Menu> menus)
        {
            var menuDtos = menus.Select(MapToDto).ToDictionary(m => m.Id, m => m);
            var roots = new List<MenuDto>();

            foreach (var menu in menuDtos.Values)
            {
                if (!string.IsNullOrEmpty(menu.ParentId) && menuDtos.TryGetValue(menu.ParentId, out var parent))
                {
                    parent.Children.Add(menu);
                }
                else
                {
                    roots.Add(menu);
                }
            }

            return roots;
        }

        private static void SortMenuTree(List<MenuDto> nodes)
        {
            nodes.Sort((a, b) => a.Order.CompareTo(b.Order));

            foreach (var node in nodes)
            {
                if (node.Children.Count > 0)
                {
                    SortMenuTree(node.Children);
                }
            }
        }

        private static void PruneEmptyDirectories(List<MenuDto> nodes)
        {
            for (var i = nodes.Count - 1; i >= 0; i--)
            {
                var node = nodes[i];

                if (node.Children.Count > 0)
                {
                    PruneEmptyDirectories(node.Children);
                }

                if (node.Type == MenuType.Directory && node.Children.Count == 0)
                {
                    nodes.RemoveAt(i);
                }
            }
        }
    }
}
