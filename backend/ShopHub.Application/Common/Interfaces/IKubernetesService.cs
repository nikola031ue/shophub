using ShopHub.Domain.Entities;

namespace ShopHub.Application.Common.Interfaces;

public interface IKubernetesService
{
    Task CreateDatabaseAsync(Store store, CancellationToken cancellationToken = default);
    Task CreateShopAsync(Store store, CancellationToken cancellationToken = default);
    Task DeleteShopAsync(Store store, CancellationToken cancellationToken = default);
    Task DeleteDatabaseAsync(Store store, CancellationToken cancellationToken = default);
}
