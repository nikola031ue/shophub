using System.Security.Claims;
using FluentAssertions;
using Moq;
using ShopHub.Application.Auth.Commands.RefreshToken;
using ShopHub.Application.Auth.Interfaces;
using ShopHub.Domain.Entities;
using ShopHub.Tests.Unit.Infrastructure;

namespace ShopHub.Tests.Unit.Auth;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IJwtService> _jwtService = new();

    [Fact]
    public async Task Handle_ShouldReturnNewToken_WhenRefreshTokenIsValid()
    {
        using var db = DbContextFactory.Create();
        var user = User.Create("user@test.com", "hash");
        user.SetRefreshToken("valid-refresh", DateTime.UtcNow.AddDays(7));
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, "user@test.com")
        ]));

        _jwtService.Setup(j => j.GetPrincipalFromExpiredToken("expired-token")).Returns(principal);
        _jwtService.Setup(j => j.GenerateAccessToken(user.Id, "user@test.com", "User")).Returns("new-access-token");
        _jwtService.Setup(j => j.GenerateRefreshToken()).Returns(("new-refresh", DateTime.UtcNow.AddDays(7)));

        var handler = new RefreshTokenCommandHandler(db, _jwtService.Object);
        var result = await handler.Handle(new RefreshTokenCommand("expired-token", "valid-refresh"), CancellationToken.None);

        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("new-access-token");
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenRefreshTokenIsExpired()
    {
        using var db = DbContextFactory.Create();
        var user = User.Create("user@test.com", "hash");
        user.SetRefreshToken("expired-refresh", DateTime.UtcNow.AddDays(-1));
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        ]));

        _jwtService.Setup(j => j.GetPrincipalFromExpiredToken("token")).Returns(principal);

        var handler = new RefreshTokenCommandHandler(db, _jwtService.Object);
        var result = await handler.Handle(new RefreshTokenCommand("token", "expired-refresh"), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenAccessTokenIsInvalid()
    {
        using var db = DbContextFactory.Create();
        _jwtService.Setup(j => j.GetPrincipalFromExpiredToken("bad-token")).Returns((ClaimsPrincipal?)null);

        var handler = new RefreshTokenCommandHandler(db, _jwtService.Object);
        var result = await handler.Handle(new RefreshTokenCommand("bad-token", "any-refresh"), CancellationToken.None);

        result.Should().BeNull();
    }
}
