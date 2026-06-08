using MediatR;
using Microsoft.EntityFrameworkCore;
using ShopHub.Application.Common.Interfaces;
using ShopHub.Application.Stores.Dtos;

namespace ShopHub.Application.Stores.Queries.GetStores;

public class GetStoresQueryHandler(IShopHubDbContext db) : IRequestHandler<GetStoresQuery, IReadOnlyList<StoreDto>>
{
    public async Task<IReadOnlyList<StoreDto>> Handle(GetStoresQuery request, CancellationToken cancellationToken)
    {
        return await db.Stores
            .Where(s => s.UserId == request.UserId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new StoreDto(s.Id, s.Name, s.Availability, s.WalletAddress, s.DatabaseType, s.Status, s.Url, s.CreatedAt, s.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
