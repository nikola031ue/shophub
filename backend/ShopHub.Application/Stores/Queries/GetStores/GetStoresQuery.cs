using MediatR;
using ShopHub.Application.Stores.Dtos;

namespace ShopHub.Application.Stores.Queries.GetStores;

public record GetStoresQuery(Guid UserId) : IRequest<IReadOnlyList<StoreDto>>;
