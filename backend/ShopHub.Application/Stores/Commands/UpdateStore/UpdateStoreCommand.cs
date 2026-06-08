using MediatR;
using ShopHub.Domain.Enums;

namespace ShopHub.Application.Stores.Commands.UpdateStore;

public record UpdateStoreCommand(
    Guid Id,
    StoreAvailability Availability,
    string WalletAddress,
    Guid UserId) : IRequest<bool>;
