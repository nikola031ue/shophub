using FluentAssertions;
using Moq;
using ShopHub.Application.Auth.Commands.Register;
using ShopHub.Application.Auth.Interfaces;
using ShopHub.Domain.Entities;
using ShopHub.Tests.Unit.Infrastructure;

namespace ShopHub.Tests.Unit.Auth;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IJwtService> _jwtService = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();

    [Fact]
    public async Task Handle_ShouldReturnToken_WhenEmailIsNew()
    {
        using var db = DbContextFactory.Create();

        _passwordHasher.Setup(p => p.Hash("Password1!")).Returns("hashed");
        _jwtService.Setup(j => j.GenerateAccessToken(It.IsAny<Guid>(), "user@test.com", "User")).Returns("access-token");
        _jwtService.Setup(j => j.GenerateRefreshToken()).Returns(("refresh-token", DateTime.UtcNow.AddDays(7)));

        var handler = new RegisterCommandHandler(db, _jwtService.Object, _passwordHasher.Object);
        var result = await handler.Handle(new RegisterCommand("user@test.com", "Password1!"), CancellationToken.None);

        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenEmailAlreadyExists()
    {
        using var db = DbContextFactory.Create();
        db.Users.Add(User.Create("user@test.com", "hashed"));
        await db.SaveChangesAsync();

        var handler = new RegisterCommandHandler(db, _jwtService.Object, _passwordHasher.Object);
        var result = await handler.Handle(new RegisterCommand("user@test.com", "Password1!"), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldPersistUser_WhenRegistrationSucceeds()
    {
        using var db = DbContextFactory.Create();
        _passwordHasher.Setup(p => p.Hash(It.IsAny<string>())).Returns("hashed");
        _jwtService.Setup(j => j.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Returns("token");
        _jwtService.Setup(j => j.GenerateRefreshToken()).Returns(("refresh", DateTime.UtcNow.AddDays(7)));

        var handler = new RegisterCommandHandler(db, _jwtService.Object, _passwordHasher.Object);
        await handler.Handle(new RegisterCommand("new@test.com", "Password1!"), CancellationToken.None);

        db.Users.Should().ContainSingle(u => u.Email == "new@test.com");
    }
}
