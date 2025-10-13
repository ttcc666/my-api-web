using MyApiWeb.Models.DTOs;
using MyApiWeb.Models.Entities;
using System.Threading.Tasks;

namespace MyApiWeb.Services.Interfaces
{
    public interface ITokenService
    {
        Task<TokenDto> GenerateTokensAsync(User user);
        Task<(bool IsSuccess, string Error, TokenDto NewTokens)> RefreshTokenAsync(string refreshToken);
        Task<bool> RevokeTokenAsync(string refreshToken);
    }
}