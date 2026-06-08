using MediatR;
using Microsoft.EntityFrameworkCore;
using ShopHub.Application.Common.Interfaces;
using ShopHub.Domain.Enums;

namespace ShopHub.Application.Stores.Commands.DeleteStore;

public class DeleteStoreCommandHandler(IShopHubDbContext db) : IRequestHandler<DeleteStoreCommand, bool>
{
    public async Task<bool> Handle(DeleteStoreCommand request, CancellationToken cancellationToken)
    {
        var store = await db.Stores
            .FirstOrDefaultAsync(s => s.Id == request.Id && s.UserId == request.UserId, cancellationToken);

        if (store is null) return false;

        store.SetStatus(StoreStatus.Deleting);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
