using MediatR;
using Microsoft.EntityFrameworkCore;
using ShopHub.Application.Common.Interfaces;

namespace ShopHub.Application.Stores.Commands.UpdateStore;

public class UpdateStoreCommandHandler(IShopHubDbContext db) : IRequestHandler<UpdateStoreCommand, bool>
{
    public async Task<bool> Handle(UpdateStoreCommand request, CancellationToken cancellationToken)
    {
        var store = await db.Stores
            .FirstOrDefaultAsync(s => s.Id == request.Id && s.UserId == request.UserId, cancellationToken);

        if (store is null) return false;

        store.Update(request.Availability, request.WalletAddress);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
