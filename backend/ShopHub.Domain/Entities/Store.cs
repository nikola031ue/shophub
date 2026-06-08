using ShopHub.Domain.Enums;

namespace ShopHub.Domain.Entities;

public class Store
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public StoreAvailability Availability { get; private set; }
    public string WalletAddress { get; private set; } = string.Empty;
    public DatabaseType DatabaseType { get; private set; }
    public StoreStatus Status { get; private set; }
    public string? Url { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Store() { }

    public static Store Create(string name, StoreAvailability availability, string walletAddress, DatabaseType databaseType, Guid userId) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Availability = availability,
            WalletAddress = walletAddress,
            DatabaseType = databaseType,
            Status = StoreStatus.Pending,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

    public void Update(StoreAvailability availability, string walletAddress)
    {
        Availability = availability;
        WalletAddress = walletAddress;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetStatus(StoreStatus status, string? url = null)
    {
        Status = status;
        if (url is not null) Url = url;
        UpdatedAt = DateTime.UtcNow;
    }
}
