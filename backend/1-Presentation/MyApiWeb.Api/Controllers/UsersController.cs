using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApiWeb.Models.DTOs;
using MyApiWeb.Services.Interfaces;
using MyApiWeb.Models.Entities;

namespace MyApiWeb.Api.Controllers
{
    public class UsersController : ApiControllerBase<IUserService, User, string>
    {
        private readonly ITokenService _tokenService;

        public UsersController(
            ILogger<UsersController> logger,
            IUserService userService,
            ITokenService tokenService)
            : base(logger, userService)
        {
            _tokenService = tokenService;
        }

        /// <summary>
        /// 用户注册
        /// </summary>
        /// <param name="registerDto">注册信息</param>
        /// <returns>用户信息</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _service.RegisterAsync(registerDto);
                return Success(user, "注册成功");
            }
            catch (InvalidOperationException ex)
            {
                return Error(ex.Message, 400);
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
        /// <param name="loginDto">登录信息</param>
        /// <returns>JWT Token</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _service.ValidateUserAsync(loginDto.Username, loginDto.Password);
                var tokens = await _tokenService.GenerateTokensAsync(user);
                
                return Success(tokens, "登录成功");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Error(ex.Message, 401);
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
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                {
                    return Error("未找到用户信息", 401);
                }

                var user = await _service.GetUserByUsernameAsync(username);
                if (user == null)
                {
                    return Error("用户不存在", 404);
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
        /// 获取所有用户（需要认证）
        /// </summary>
        /// <returns>用户列表</returns>
        [HttpGet]
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
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            try
            {
                var user = await _service.GetUserByIdAsync(id);
                if (user == null)
                {
                    return Error("用户不存在", 404);
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
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserUpdateDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _service.UpdateUserAsync(id, updateDto);
                if (!result)
                {
                    return Error("用户不存在", 404);
                }

                return Success("更新成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新用户信息失败");
                return Error("服务器内部错误");
            }
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>操作结果</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var result = await _service.DeleteUserAsync(id);
                if (!result)
                {
                    return Error("用户不存在", 404);
                }

                return Success("删除成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除用户失败");
                return Error("服务器内部错误");
            }
        }
    }
}