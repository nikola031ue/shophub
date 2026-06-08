using MediatR;
using Microsoft.EntityFrameworkCore;
using ShopHub.Application.Common.Interfaces;
using ShopHub.Application.Stores.Dtos;
using ShopHub.Domain.Enums;

namespace ShopHub.Application.Stores.Queries.GetStoreById;

public class GetStoreByIdQueryHandler(IShopHubDbContext db) : IRequestHandler<GetStoreByIdQuery, StoreDto?>
{
    public async Task<StoreDto?> Handle(GetStoreByIdQuery request, CancellationToken cancellationToken)
    {
        var store = await db.Stores
            .FirstOrDefaultAsync(s => s.Id == request.Id && s.UserId == request.UserId && s.Status != StoreStatus.Deleting, cancellationToken);

        return store is null ? null
            : new StoreDto(store.Id, store.Name, store.Availability, store.WalletAddress, store.DatabaseType, store.Status, store.Url, store.CreatedAt, store.UpdatedAt);
    }
}
