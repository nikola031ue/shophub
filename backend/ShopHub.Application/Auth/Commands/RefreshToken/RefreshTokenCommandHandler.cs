using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ShopHub.Application.Auth.Dtos;
using ShopHub.Application.Auth.Interfaces;
using ShopHub.Application.Common.Interfaces;

namespace ShopHub.Application.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler(IShopHubDbContext db, IJwtService jwtService)
    : IRequestHandler<RefreshTokenCommand, TokenDto?>
{
    public async Task<TokenDto?> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var principal = jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal is null) return null;

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId)) return null;

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null || !user.IsRefreshTokenValid(request.RefreshToken))
            return null;

        var newAccessToken = jwtService.GenerateAccessToken(user.Id, user.Email, "User");
        var (newRefreshToken, refreshExpiry) = jwtService.GenerateRefreshToken();

        user.SetRefreshToken(newRefreshToken, refreshExpiry);
        await db.SaveChangesAsync(cancellationToken);

        return new TokenDto(newAccessToken, newRefreshToken, DateTime.UtcNow.AddMinutes(60));
    }
}
