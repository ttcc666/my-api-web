using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MyApiWeb.Models.DTOs;
using MyApiWeb.Models.Entities;
using MyApiWeb.Repository.Interfaces;
using MyApiWeb.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MyApiWeb.Services.Implements
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly IRepository<RefreshToken> _refreshTokenRepo;
        private readonly IRepository<User> _userRepo;

        public TokenService(IConfiguration configuration, IRepository<RefreshToken> refreshTokenRepo, IRepository<User> userRepo)
        {
            _configuration = configuration;
            _refreshTokenRepo = refreshTokenRepo;
            _userRepo = userRepo;
        }

        public async Task<TokenDto> GenerateTokensAsync(User user)
        {
            var jwtId = Guid.NewGuid().ToString();
            var accessToken = GenerateAccessToken(user, jwtId);
            var refreshToken = await GenerateAndStoreRefreshTokenAsync(user.Id, jwtId);

            return new TokenDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            };
        }

        public async Task<(bool IsSuccess, string Error, TokenDto NewTokens)> RefreshTokenAsync(string refreshToken)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(refreshToken))
                return (false, "Refresh token is required.", null!);

            // Prevent overly long input
            if (refreshToken.Length > 1000)
                return (false, "Invalid refresh token format.", null!);

            var dbToken = await _refreshTokenRepo.FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (dbToken == null)
                return (false, "Invalid refresh token.", null!);

            if (dbToken.IsUsed || dbToken.IsRevoked)
                return (false, "Refresh token has been used or revoked.", null!);

            if (dbToken.ExpiresAt < DateTime.UtcNow)
                return (false, "Refresh token has expired.", null!);

            // 令牌轮换：立即使当前令牌失效
            dbToken.IsUsed = true;
            await _refreshTokenRepo.UpdateAsync(dbToken);

            var user = await _userRepo.GetByIdAsync(dbToken.UserId);
            if (user == null)
                return (false, "User not found.", null!);

            var newTokens = await GenerateTokensAsync(user);
            return (true, string.Empty, newTokens);
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return false;

            var dbToken = await _refreshTokenRepo.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
            if (dbToken == null || dbToken.IsRevoked)
                return false;

            dbToken.IsRevoked = true;
            dbToken.IsUsed = true; // 登出时也标记为已使用
            await _refreshTokenRepo.UpdateAsync(dbToken);

            return true;
        }

        private string GenerateAccessToken(User user, string jwtId)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, jwtId),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                // 根据需要添加更多声明，如角色
                // new Claim(ClaimTypes.Role, user.Role)
            };

            var secret = _configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:AccessTokenExpirationMinutes"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<RefreshToken> GenerateAndStoreRefreshTokenAsync(string userId, string jwtId)
        {
            var refreshToken = new RefreshToken
            {
                Token = GenerateRandomTokenString(),
                JwtId = jwtId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["JwtSettings:RefreshTokenExpirationDays"])),
            };

            await _refreshTokenRepo.InsertAsync(refreshToken);
            return refreshToken;
        }

        private string GenerateRandomTokenString()
        {
            using var rng = RandomNumberGenerator.Create();
            var randomBytes = new byte[64];
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}