using System.ComponentModel.DataAnnotations;

namespace MyApiWeb.Models.DTOs
{
    /// <summary>
    /// 用户数据传输对象
    /// </summary>
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? RealName { get; set; }
        public string? Phone { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? LastLoginTime { get; set; }
    }

    /// <summary>
    /// 用户注册DTO
    /// </summary>
    public class UserRegisterDto
    {
        [Required(ErrorMessage = "用户名不能为空")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "用户名长度必须在3-50个字符之间")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "邮箱不能为空")]
        [EmailAddress(ErrorMessage = "邮箱格式不正确")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "密码不能为空")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "密码长度必须在6-100个字符之间")]
        public string Password { get; set; } = string.Empty;

        [DisplayFormat(ConvertEmptyStringToNull = true)]
        [StringLength(100, ErrorMessage = "真实姓名不能超过100个字符")]
        public string? RealName { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = true)]
        [Phone(ErrorMessage = "手机号格式不正确")]
        public string? Phone { get; set; }
    }

    /// <summary>
    /// 用户登录DTO
    /// </summary>
    public class UserLoginDto
    {
        [Required(ErrorMessage = "用户名不能为空")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "密码不能为空")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// 用户更新DTO
    /// </summary>
    public class UserUpdateDto
    {
        [DisplayFormat(ConvertEmptyStringToNull = true)]
        [StringLength(100, ErrorMessage = "真实姓名不能超过100个字符")]
        public string? RealName { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = true)]
        [Phone(ErrorMessage = "手机号格式不正确")]
        public string? Phone { get; set; }
    }

    /// <summary>
    /// 修改密码 DTO
    /// </summary>
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "当前密码不能为空")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "密码长度必须在6-100个字符之间")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "新密码不能为空")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "密码长度必须在6-100个字符之间")]
        public string NewPassword { get; set; } = string.Empty;
    }
}