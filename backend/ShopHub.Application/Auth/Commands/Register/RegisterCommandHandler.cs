using MediatR;
using Microsoft.EntityFrameworkCore;
using ShopHub.Application.Auth.Dtos;
using ShopHub.Application.Auth.Interfaces;
using ShopHub.Application.Common.Interfaces;
using ShopHub.Domain.Entities;

namespace ShopHub.Application.Auth.Commands.Register;

public class RegisterCommandHandler(IShopHubDbContext db, IJwtService jwtService, IPasswordHasher passwordHasher)
    : IRequestHandler<RegisterCommand, TokenDto?>
{
    public async Task<TokenDto?> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var exists = await db.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);
        if (exists) return null;

        var user = User.Create(request.Email, passwordHasher.Hash(request.Password));
        db.Users.Add(user);

        var accessToken = jwtService.GenerateAccessToken(user.Id, user.Email, "User");
        var (refreshToken, refreshExpiry) = jwtService.GenerateRefreshToken();
        user.SetRefreshToken(refreshToken, refreshExpiry);

        await db.SaveChangesAsync(cancellationToken);

        return new TokenDto(accessToken, refreshToken, DateTime.UtcNow.AddMinutes(60));
    }
}
