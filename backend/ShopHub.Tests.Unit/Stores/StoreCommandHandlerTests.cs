using FluentAssertions;
using ShopHub.Application.Stores.Commands.CreateStore;
using ShopHub.Application.Stores.Commands.DeleteStore;
using ShopHub.Application.Stores.Commands.UpdateStore;
using ShopHub.Application.Stores.Queries.GetStoreById;
using ShopHub.Application.Stores.Queries.GetStores;
using ShopHub.Domain.Entities;
using ShopHub.Domain.Enums;
using ShopHub.Tests.Unit.Infrastructure;

namespace ShopHub.Tests.Unit.Stores;

public class StoreCommandHandlerTests
{
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public async Task CreateStore_ShouldPersistAndReturnId()
    {
        using var db = DbContextFactory.Create();
        var handler = new CreateStoreCommandHandler(db);

        var id = await handler.Handle(
            new CreateStoreCommand("My Shop", StoreAvailability.Standard, "0xABC", DatabaseType.Standard, _userId),
            CancellationToken.None);

        id.Should().NotBeEmpty();
        db.Stores.Should().ContainSingle(s => s.Id == id && s.UserId == _userId);
    }

    [Fact]
    public async Task CreateStore_ShouldSetStatusToPending()
    {
        using var db = DbContextFactory.Create();
        var handler = new CreateStoreCommandHandler(db);

        var id = await handler.Handle(
            new CreateStoreCommand("Shop", StoreAvailability.High, "0xABC", DatabaseType.Light, _userId),
            CancellationToken.None);

        db.Stores.First(s => s.Id == id).Status.Should().Be(StoreStatus.Pending);
    }

    [Fact]
    public async Task UpdateStore_ShouldReturnTrue_WhenStoreExists()
    {
        using var db = DbContextFactory.Create();
        var store = Store.Create("Old", StoreAvailability.Standard, "0xOLD", DatabaseType.Standard, _userId);
        db.Stores.Add(store);
        await db.SaveChangesAsync();

        var handler = new UpdateStoreCommandHandler(db);
        var result = await handler.Handle(
            new UpdateStoreCommand(store.Id, StoreAvailability.High, "0xNEW", _userId),
            CancellationToken.None);

        result.Should().BeTrue();
        db.Stores.First().WalletAddress.Should().Be("0xNEW");
        db.Stores.First().Availability.Should().Be(StoreAvailability.High);
    }

    [Fact]
    public async Task UpdateStore_ShouldReturnFalse_WhenStoreNotFound()
    {
        using var db = DbContextFactory.Create();
        var handler = new UpdateStoreCommandHandler(db);

        var result = await handler.Handle(
            new UpdateStoreCommand(Guid.NewGuid(), StoreAvailability.High, "0xABC", _userId),
            CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateStore_ShouldReturnFalse_WhenStoreBelongsToOtherUser()
    {
        using var db = DbContextFactory.Create();
        var store = Store.Create("Shop", StoreAvailability.Standard, "0xABC", DatabaseType.Standard, Guid.NewGuid());
        db.Stores.Add(store);
        await db.SaveChangesAsync();

        var handler = new UpdateStoreCommandHandler(db);
        var result = await handler.Handle(
            new UpdateStoreCommand(store.Id, StoreAvailability.High, "0xNEW", _userId),
            CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteStore_ShouldRemoveStore_WhenExists()
    {
        using var db = DbContextFactory.Create();
        var store = Store.Create("Shop", StoreAvailability.Standard, "0xABC", DatabaseType.Standard, _userId);
        db.Stores.Add(store);
        await db.SaveChangesAsync();

        var handler = new DeleteStoreCommandHandler(db);
        var result = await handler.Handle(new DeleteStoreCommand(store.Id, _userId), CancellationToken.None);

        result.Should().BeTrue();
        db.Stores.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteStore_ShouldReturnFalse_WhenStoreBelongsToOtherUser()
    {
        using var db = DbContextFactory.Create();
        var store = Store.Create("Shop", StoreAvailability.Standard, "0xABC", DatabaseType.Standard, Guid.NewGuid());
        db.Stores.Add(store);
        await db.SaveChangesAsync();

        var handler = new DeleteStoreCommandHandler(db);
        var result = await handler.Handle(new DeleteStoreCommand(store.Id, _userId), CancellationToken.None);

        result.Should().BeFalse();
        db.Stores.Should().ContainSingle();
    }

    [Fact]
    public async Task GetStores_ShouldReturnOnlyUserStores()
    {
        using var db = DbContextFactory.Create();
        db.Stores.Add(Store.Create("My Shop", StoreAvailability.Standard, "0xA", DatabaseType.Standard, _userId));
        db.Stores.Add(Store.Create("Other Shop", StoreAvailability.Standard, "0xB", DatabaseType.Standard, Guid.NewGuid()));
        await db.SaveChangesAsync();

        var handler = new GetStoresQueryHandler(db);
        var result = await handler.Handle(new GetStoresQuery(_userId), CancellationToken.None);

        result.Should().ContainSingle(s => s.Name == "My Shop");
    }

    [Fact]
    public async Task GetStoreById_ShouldReturnNull_WhenStoreBelongsToOtherUser()
    {
        using var db = DbContextFactory.Create();
        var store = Store.Create("Shop", StoreAvailability.Standard, "0xABC", DatabaseType.Standard, Guid.NewGuid());
        db.Stores.Add(store);
        await db.SaveChangesAsync();

        var handler = new GetStoreByIdQueryHandler(db);
        var result = await handler.Handle(new GetStoreByIdQuery(store.Id, _userId), CancellationToken.None);

        result.Should().BeNull();
    }
}
