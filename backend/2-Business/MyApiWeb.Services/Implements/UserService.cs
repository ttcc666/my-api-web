using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MyApiWeb.Models.DTOs;
using MyApiWeb.Models.Entities;
using MyApiWeb.Repository.Interfaces;
using MyApiWeb.Services.Interfaces;

namespace MyApiWeb.Services.Implements
{
    /// <summary>
    /// 用户服务实现
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IRepository<User> userRepository,
            IConfiguration configuration,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<User> ValidateUserAsync(string username, string password)
        {
            var user = await _userRepository.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null || !VerifyPassword(password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("用户名或密码错误");
            }

            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException("用户已被禁用");
            }

            // 更新最后登录时间
            user.LastLoginTime = DateTime.Now;
            await _userRepository.UpdateAsync(user);

            return user;
        }

        public async Task<UserDto> RegisterAsync(UserRegisterDto registerDto)
        {
            try
            {
                // 检查用户名是否已存在
                if (await UsernameExistsAsync(registerDto.Username))
                {
                    throw new InvalidOperationException("用户名已存在");
                }

                // 检查邮箱是否已存在
                if (await EmailExistsAsync(registerDto.Email))
                {
                    throw new InvalidOperationException("邮箱已存在");
                }

                // 创建新用户
                var user = new User
                {
                    Username = registerDto.Username,
                    Email = registerDto.Email,
                    PasswordHash = HashPassword(registerDto.Password),
                    RealName = registerDto.RealName,
                    Phone = registerDto.Phone,
                    IsActive = true
                };

                await _userRepository.InsertAsync(user);

                return MapToUserDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "用户注册失败: {Username}", registerDto.Username);
                throw;
            }
        }

        public async Task<UserDto?> GetUserByIdAsync(string id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user == null ? null : MapToUserDto(user);
        }

        public async Task<UserDto?> GetUserByUsernameAsync(string username)
        {
            var user = await _userRepository.FirstOrDefaultAsync(u => u.Username == username);
            return user == null ? null : MapToUserDto(user);
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(MapToUserDto).ToList();
        }

        public async Task<bool> UpdateUserAsync(string id, UserUpdateDto updateDto)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return false;
                }

                user.RealName = updateDto.RealName;
                user.Phone = updateDto.Phone;
                user.UpdatedTime = DateTime.Now;

                return await _userRepository.UpdateAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新用户失败: {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            try
            {
                return await _userRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除用户失败: {Id}", id);
                throw;
            }
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _userRepository.ExistsAsync(u => u.Username == username);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _userRepository.ExistsAsync(u => u.Email == email);
        }

        public bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }


        private static UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                RealName = user.RealName,
                Phone = user.Phone,
                IsActive = user.IsActive,
                CreatedTime = user.CreationTime.DateTime,
                LastLoginTime = user.LastLoginTime
            };
        }
    }
}