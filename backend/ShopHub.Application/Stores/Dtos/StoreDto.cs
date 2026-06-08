using ShopHub.Domain.Enums;

namespace ShopHub.Application.Stores.Dtos;

public record StoreDto(
    Guid Id,
    string Name,
    StoreAvailability Availability,
    string WalletAddress,
    DatabaseType DatabaseType,
    StoreStatus Status,
    string? Url,
    DateTime CreatedAt,
    DateTime UpdatedAt);
