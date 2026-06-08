using MediatR;
using ShopHub.Application.Common.Interfaces;
using ShopHub.Domain.Entities;

namespace ShopHub.Application.Stores.Commands.CreateStore;

public class CreateStoreCommandHandler(IShopHubDbContext db) : IRequestHandler<CreateStoreCommand, Guid>
{
    public async Task<Guid> Handle(CreateStoreCommand request, CancellationToken cancellationToken)
    {
        var store = Store.Create(request.Name, request.Availability, request.WalletAddress, request.DatabaseType, request.UserId);
        db.Stores.Add(store);
        await db.SaveChangesAsync(cancellationToken);
        return store.Id;
    }
}
