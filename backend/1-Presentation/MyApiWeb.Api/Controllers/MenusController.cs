using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApiWeb.Models.DTOs;
using MyApiWeb.Models.Entities;
using MyApiWeb.Services.Interfaces;

namespace MyApiWeb.Api.Controllers
{
    /// <summary>
    /// 菜单管理控制器
    /// </summary>
    public class MenusController : ApiControllerBase<IMenuService, Menu, string>
    {
        public MenusController(ILogger<MenusController> logger, IMenuService menuService)
            : base(logger, menuService)
        {
        }

        /// <summary>
        /// 获取菜单列表
        /// </summary>
        /// <param name="onlyEnabled">是否仅返回启用菜单</param>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<MenuDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMenus([FromQuery] bool onlyEnabled = false)
        {
            var menus = await _service.GetAllMenusAsync(onlyEnabled);
            return Success(menus);
        }

        /// <summary>
        /// 获取菜单树
        /// </summary>
        /// <param name="onlyEnabled">是否仅返回启用菜单</param>
        [HttpGet("tree")]
        [ProducesResponseType(typeof(ApiResponse<List<MenuDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMenuTree([FromQuery] bool onlyEnabled = false)
        {
            var menus = await _service.GetMenuTreeAsync(onlyEnabled);
            return Success(menus);
        }

        /// <summary>
        /// 根据ID获取菜单
        /// </summary>
        /// <param name="id">菜单ID</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<MenuDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMenuById(string id)
        {
            var menu = await _service.GetMenuByIdAsync(id);
            if (menu == null)
            {
                return Error("菜单不存在", StatusCodes.Status404NotFound);
            }

            return Success(menu);
        }

        /// <summary>
        /// 创建菜单
        /// </summary>
        /// <param name="createMenuDto">菜单创建参数</param>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<MenuDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateMenu([FromBody] CreateMenuDto createMenuDto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationError(ModelState);
            }

            var menu = await _service.CreateMenuAsync(createMenuDto);
            return Success(menu, "菜单创建成功");
        }

        /// <summary>
        /// 更新菜单
        /// </summary>
        /// <param name="id">菜单ID</param>
        /// <param name="updateMenuDto">菜单更新参数</param>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<MenuDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateMenu(string id, [FromBody] UpdateMenuDto updateMenuDto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationError(ModelState);
            }

            var menu = await _service.UpdateMenuAsync(id, updateMenuDto);
            return Success(menu, "菜单更新成功");
        }

        /// <summary>
        /// 删除菜单
        /// </summary>
        /// <param name="id">菜单ID</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteMenu(string id)
        {
            var result = await _service.DeleteMenuAsync(id);
            if (result)
            {
                return SuccessMessage("菜单删除成功");
            }

            return Error("菜单删除失败", StatusCodes.Status400BadRequest);
        }
    }
}
