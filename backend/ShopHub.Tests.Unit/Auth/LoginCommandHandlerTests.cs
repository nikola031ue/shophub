using FluentAssertions;
using Moq;
using ShopHub.Application.Auth.Commands.Login;
using ShopHub.Application.Auth.Interfaces;
using ShopHub.Domain.Entities;
using ShopHub.Tests.Unit.Infrastructure;

namespace ShopHub.Tests.Unit.Auth;

public class LoginCommandHandlerTests
{
    private readonly Mock<IJwtService> _jwtService = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();

    [Fact]
    public async Task Handle_ShouldReturnToken_WhenCredentialsAreValid()
    {
        using var db = DbContextFactory.Create();
        db.Users.Add(User.Create("user@test.com", "hashed-password"));
        await db.SaveChangesAsync();

        _passwordHasher.Setup(p => p.Verify("Password1!", "hashed-password")).Returns(true);
        _jwtService.Setup(j => j.GenerateAccessToken(It.IsAny<Guid>(), "user@test.com", "User")).Returns("access-token");
        _jwtService.Setup(j => j.GenerateRefreshToken()).Returns(("refresh-token", DateTime.UtcNow.AddDays(7)));

        var handler = new LoginCommandHandler(db, _jwtService.Object, _passwordHasher.Object);
        var result = await handler.Handle(new LoginCommand("user@test.com", "Password1!"), CancellationToken.None);

        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("access-token");
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenUserNotFound()
    {
        using var db = DbContextFactory.Create();
        var handler = new LoginCommandHandler(db, _jwtService.Object, _passwordHasher.Object);

        var result = await handler.Handle(new LoginCommand("unknown@test.com", "password"), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenPasswordIsWrong()
    {
        using var db = DbContextFactory.Create();
        db.Users.Add(User.Create("user@test.com", "hashed-password"));
        await db.SaveChangesAsync();

        _passwordHasher.Setup(p => p.Verify("wrongpassword", "hashed-password")).Returns(false);
        var handler = new LoginCommandHandler(db, _jwtService.Object, _passwordHasher.Object);

        var result = await handler.Handle(new LoginCommand("user@test.com", "wrongpassword"), CancellationToken.None);

        result.Should().BeNull();
    }
}
