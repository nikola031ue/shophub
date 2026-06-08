using ShopHub.Domain.Entities;

namespace ShopHub.Application.Common.Interfaces;

public interface IKubernetesService
{
    Task CreateShopAsync(Store store, CancellationToken cancellationToken = default);
}
