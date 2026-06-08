using MediatR;
using ShopHub.Application.Stores.Dtos;

namespace ShopHub.Application.Stores.Queries.GetStoreById;

public record GetStoreByIdQuery(Guid Id, Guid UserId) : IRequest<StoreDto?>;
