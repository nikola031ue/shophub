using MediatR;
using Microsoft.EntityFrameworkCore;
using ShopHub.Application.Auth.Dtos;
using ShopHub.Application.Auth.Interfaces;
using ShopHub.Application.Common.Interfaces;

namespace ShopHub.Application.Auth.Commands.Login;

public class LoginCommandHandler(IShopHubDbContext db, IJwtService jwtService, IPasswordHasher passwordHasher)
    : IRequestHandler<LoginCommand, TokenDto?>
{
    public async Task<TokenDto?> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            return null;

        var accessToken = jwtService.GenerateAccessToken(user.Id, user.Email, "User");
        var (refreshToken, refreshExpiry) = jwtService.GenerateRefreshToken();

        user.SetRefreshToken(refreshToken, refreshExpiry);
        await db.SaveChangesAsync(cancellationToken);

        return new TokenDto(accessToken, refreshToken, DateTime.UtcNow.AddMinutes(60));
    }
}
