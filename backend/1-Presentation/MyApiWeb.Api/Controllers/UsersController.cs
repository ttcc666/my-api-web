using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApiWeb.Models.DTOs;
using MyApiWeb.Models.Entities.System;
using MyApiWeb.Services.Interfaces.System;
using MyApiWeb.Services.Interfaces.Auth;

namespace MyApiWeb.Api.Controllers
{
    /// <summary>
    /// 用户管理控制器
    /// </summary>
    /// <remarks>
    /// 提供用户注册、登录、信息管理和角色分配功能。
    /// 包含公开接口（注册、登录）和需要认证的接口（用户管理）。
    /// </remarks>
    public class UsersController : ApiControllerBase<IUserService, User, string>
    {
        private readonly ITokenService _tokenService;
        private readonly IRoleService _roleService;

        /// <summary>
        /// 初始化用户管理控制器
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <param name="userService">用户服务</param>
        /// <param name="tokenService">令牌服务</param>
        /// <param name="roleService">角色服务</param>
        public UsersController(
            ILogger<UsersController> logger,
            IUserService userService,
            ITokenService tokenService,
            IRoleService roleService)
            : base(logger, userService)
        {
            _tokenService = tokenService;
            _roleService = roleService;
        }

        /// <summary>
        /// 用户注册
        /// </summary>
        /// <param name="registerDto">注册信息（包含用户名、密码等）</param>
        /// <returns>用户信息</returns>
        /// <response code="200">注册成功，返回用户信息</response>
        /// <response code="400">请求参数无效或用户名已存在</response>
        /// <response code="500">服务器内部错误</response>
        /// <remarks>
        /// 公开接口，无需认证。
        /// 用户名必须唯一，密码会自动加密存储。
        /// </remarks>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                var user = await _service.RegisterAsync(registerDto);
                return Success(user, "注册成功");
            }
            catch (InvalidOperationException ex)
            {
                return Error(ex.Message, StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "用户注册失败");
                return Error("服务器内部错误");
            }
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="loginDto">登录信息（包含用户名和密码）</param>
        /// <returns>JWT Token（包含访问令牌和刷新令牌）</returns>
        /// <response code="200">登录成功，返回JWT令牌对</response>
        /// <response code="401">用户名或密码错误</response>
        /// <response code="400">请求参数无效</response>
        /// <response code="500">服务器内部错误</response>
        /// <remarks>
        /// 公开接口，无需认证。
        /// 验证用户名和密码后，生成访问令牌（Access Token）和刷新令牌（Refresh Token）。
        /// 客户端应妥善保存令牌，并在后续请求中携带访问令牌。
        /// </remarks>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<TokenDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                var user = await _service.ValidateUserAsync(loginDto.Username, loginDto.Password, loginDto.CaptchaCode);
                var tokens = await _tokenService.GenerateTokensAsync(user);

                return Success(tokens, "登录成功");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Error(ex.Message, StatusCodes.Status401Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "用户登录失败");
                return Error("服务器内部错误");
            }
        }

        /// <summary>
        /// 获取当前用户信息
        /// </summary>
        /// <returns>用户信息</returns>
        /// <response code="200">成功返回当前用户信息</response>
        /// <response code="401">用户未认证</response>
        /// <response code="404">用户不存在</response>
        /// <response code="500">服务器内部错误</response>
        /// <remarks>
        /// 需要认证。根据JWT令牌中的用户信息返回当前登录用户的详细信息。
        /// </remarks>
        [HttpGet("profile")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                {
                    return Error("未找到用户信息", StatusCodes.Status401Unauthorized);
                }

                var user = await _service.GetUserByUsernameAsync(username);
                if (user == null)
                {
                    return Error("用户不存在", StatusCodes.Status404NotFound);
                }

                return Success(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户信息失败");
                return Error("服务器内部错误");
            }
        }

        /// <summary>
        /// 获取所有用户
        /// </summary>
        /// <returns>用户列表</returns>
        /// <response code="200">成功返回所有用户列表</response>
        /// <response code="500">服务器内部错误</response>
        /// <remarks>
        /// 需要认证。返回系统中所有用户的信息。
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<UserDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _service.GetAllUsersAsync();
                return Success(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户列表失败");
                return Error("服务器内部错误");
            }
        }

        /// <summary>
        /// 根据ID获取用户
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>用户信息</returns>
        /// <response code="200">成功返回用户信息</response>
        /// <response code="404">用户不存在</response>
        /// <response code="500">服务器内部错误</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserById(string id)
        {
            try
            {
                var user = await _service.GetUserByIdAsync(id);
                if (user == null)
                {
                    return Error("用户不存在", StatusCodes.Status404NotFound);
                }

                return Success(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户信息失败");
                return Error("服务器内部错误");
            }
        }

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="updateDto">更新信息</param>
        /// <returns>操作结果</returns>
        /// <response code="200">更新成功</response>
        /// <response code="400">请求参数无效</response>
        /// <response code="404">用户不存在</response>
        /// <response code="500">服务器内部错误</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserUpdateDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                var result = await _service.UpdateUserAsync(id, updateDto);
                if (!result)
                {
                    return Error("用户不存在", StatusCodes.Status404NotFound);
                }

                return SuccessMessage("更新成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新用户信息失败");
                return Error("服务器内部错误");
            }
        }

        /// <summary>
        /// 修改用户密码
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="changePasswordDto">密码修改信息</param>
        /// <returns>操作结果</returns>
        /// <response code="200">密码修改成功</response>
        /// <response code="400">请求参数无效或校验失败</response>
        /// <response code="401">用户未认证</response>
        /// <response code="403">无权限修改该用户密码</response>
        /// <response code="404">用户不存在</response>
        /// <response code="500">服务器内部错误</response>
        [HttpPut("{id}/password")]
        [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                if (CurrentUserId == null)
                {
                    return Error("未找到用户信息", StatusCodes.Status401Unauthorized);
                }

                if (!string.Equals(CurrentUserId, id, StringComparison.OrdinalIgnoreCase))
                {
                    return Error("无权限修改该用户密码", StatusCodes.Status403Forbidden);
                }

                var result = await _service.ChangePasswordAsync(id, changePasswordDto);
                if (!result)
                {
                    return Error("用户不存在", StatusCodes.Status404NotFound);
                }

                return SuccessMessage("密码修改成功");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Error(ex.Message, StatusCodes.Status400BadRequest);
            }
            catch (InvalidOperationException ex)
            {
                return Error(ex.Message, StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "修改用户密码失败");
                return Error("服务器内部错误");
            }
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>操作结果</returns>
        /// <response code="200">删除成功</response>
        /// <response code="404">用户不存在</response>
        /// <response code="500">服务器内部错误</response>
        /// <remarks>
        /// 删除用户会同时移除所有相关的用户角色和用户权限关联。
        /// </remarks>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var result = await _service.DeleteUserAsync(id);
                if (!result)
                {
                    return Error("用户不存在", StatusCodes.Status404NotFound);
                }

                return SuccessMessage("删除成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除用户失败");
                return Error("服务器内部错误");
            }
        }

        /// <summary>
        /// 获取用户的角色列表
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>角色列表</returns>
        /// <response code="200">成功返回用户的角色列表</response>
        /// <response code="500">服务器内部错误</response>
        [HttpGet("{id}/roles")]
        [ProducesResponseType(typeof(ApiResponse<List<RoleDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserRoles(string id)
        {
            try
            {
                var roles = await _roleService.GetUserRolesAsync(id);
                return Success(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户角色失败");
                return Error("服务器内部错误");
            }
        }

        /// <summary>
        /// 为用户分配角色
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="assignRolesDto">角色分配请求（包含角色ID列表）</param>
        /// <returns>分配结果</returns>
        /// <response code="200">角色分配成功</response>
        /// <response code="400">请求参数无效或分配失败</response>
        /// <response code="500">服务器内部错误</response>
        /// <remarks>
        /// 为用户分配角色，会替换用户现有的所有角色。
        /// 用户将自动继承所分配角色的所有权限。
        /// </remarks>
        [HttpPut("{id}/roles")]
        [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AssignRolesToUser(string id, [FromBody] AssignUserRolesDto assignRolesDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                var result = await _roleService.AssignRolesToUserAsync(id, assignRolesDto);
                if (result)
                {
                    return SuccessMessage("角色分配成功");
                }

                return Error("角色分配失败", StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "分配用户角色失败");
                return Error("服务器内部错误");
            }
        }

        /// <summary>
        /// 移除用户的角色
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="roleId">角色ID</param>
        /// <returns>移除结果</returns>
        /// <response code="200">角色移除成功</response>
        /// <response code="400">角色移除失败</response>
        /// <response code="500">服务器内部错误</response>
        /// <remarks>
        /// 移除用户的单个角色。用户将失去该角色的所有权限（除非通过其他角色或直接权限拥有）。
        /// </remarks>
        [HttpDelete("{id}/roles/{roleId}")]
        [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveRoleFromUser(string id, string roleId)
        {
            try
            {
                var result = await _roleService.RemoveRoleFromUserAsync(id, roleId);
                if (result)
                {
                    return SuccessMessage("角色移除成功");
                }

                return Error("角色移除失败", StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "移除用户角色失败");
                return Error("服务器内部错误");
            }
        }
    }
}
