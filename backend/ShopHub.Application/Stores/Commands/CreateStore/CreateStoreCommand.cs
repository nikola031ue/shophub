using MediatR;
using ShopHub.Domain.Enums;

namespace ShopHub.Application.Stores.Commands.CreateStore;

public record CreateStoreCommand(
    string Name,
    StoreAvailability Availability,
    string WalletAddress,
    DatabaseType DatabaseType,
    Guid UserId) : IRequest<Guid>;
