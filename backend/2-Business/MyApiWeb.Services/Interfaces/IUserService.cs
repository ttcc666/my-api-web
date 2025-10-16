using MyApiWeb.Models.DTOs;
using MyApiWeb.Models.Entities;

namespace MyApiWeb.Services.Interfaces
{
    /// <summary>
    /// 用户服务接口
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// 验证用户凭据
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns>验证成功则返回用户实体，否则返回null</returns>
        Task<User> ValidateUserAsync(string username, string password);

        /// <summary>
        /// 用户注册
        /// </summary>
        /// <param name="registerDto">注册信息</param>
        /// <returns>用户信息</returns>
        Task<UserDto> RegisterAsync(UserRegisterDto registerDto);

        /// <summary>
        /// 根据ID获取用户信息
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>用户信息</returns>
        Task<UserDto?> GetUserByIdAsync(string id);

        /// <summary>
        /// 根据用户名获取用户信息
        /// </summary>
        /// <param name="username">用户名</param>
        /// <returns>用户信息</returns>
        Task<UserDto?> GetUserByUsernameAsync(string username);

        /// <summary>
        /// 获取所有用户信息
        /// </summary>
        /// <returns>用户列表</returns>
        Task<List<UserDto>> GetAllUsersAsync();

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="updateDto">更新信息</param>
        /// <returns>是否成功</returns>
        Task<bool> UpdateUserAsync(string id, UserUpdateDto updateDto);

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>是否成功</returns>
        Task<bool> DeleteUserAsync(string id);

        /// <summary>
        /// 检查用户名是否存在
        /// </summary>
        /// <param name="username">用户名</param>
        /// <returns>是否存在</returns>
        Task<bool> UsernameExistsAsync(string username);

        /// <summary>
        /// 检查邮箱是否存在
        /// </summary>
        /// <param name="email">邮箱</param>
        /// <returns>是否存在</returns>
        Task<bool> EmailExistsAsync(string email);

        /// <summary>
        /// 验证密码
        /// </summary>
        /// <param name="password">明文密码</param>
        /// <param name="hash">密码哈希</param>
        /// <returns>是否匹配</returns>
        bool VerifyPassword(string password, string hash);

        /// <summary>
        /// 哈希密码
        /// </summary>
        /// <param name="password">明文密码</param>
        /// <returns>密码哈希</returns>
        string HashPassword(string password);

        /// <summary>
        /// 修改用户密码
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="changePasswordDto">密码修改信息</param>
        /// <returns>是否成功</returns>
        Task<bool> ChangePasswordAsync(string id, ChangePasswordDto changePasswordDto);
    }
}