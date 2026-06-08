using System.Security.Claims;

namespace ShopHub.Application.Auth.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(Guid userId, string email, string role);
    (string Token, DateTime Expiry) GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
