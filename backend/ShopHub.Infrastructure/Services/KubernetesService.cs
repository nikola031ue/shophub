using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShopHub.Application.Common.Interfaces;
using ShopHub.Domain.Entities;
using ShopHub.Domain.Enums;
using ShopHub.Infrastructure.Kubernetes;
using ShopHub.Infrastructure.Settings;

namespace ShopHub.Infrastructure.Services;

public class KubernetesService(IOptions<KubernetesSettings> options, ILogger<KubernetesService> logger)
    : IKubernetesService
{
    private readonly KubernetesSettings _settings = options.Value;

    public async Task CreateShopAsync(Store store, CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled)
        {
            logger.LogInformation("Kubernetes integration disabled. Skipping Shop CR creation for store {StoreId}.", store.Id);
            return;
        }

        try
        {
            var client = BuildClient();
            var namespaceName = BuildNamespace(store.UserId);

            await EnsureNamespaceAsync(client, namespaceName, cancellationToken);

            var cr = BuildShopCr(store, namespaceName);
            await client.CustomObjects.CreateNamespacedCustomObjectAsync(
                cr,
                _settings.Group,
                _settings.Version,
                namespaceName,
                _settings.Plural,
                cancellationToken: cancellationToken);

            logger.LogInformation(
                "Created Shop CR {Name} in namespace {Namespace} for store {StoreId}.",
                cr.Metadata.Name, namespaceName, store.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create Shop CR for store {StoreId}. Store remains in Pending state.", store.Id);
        }
    }

    private string BuildNamespace(Guid userId) =>
        $"{_settings.NamespacePrefix}-{userId}";

    private ShopCustomResource BuildShopCr(Store store, string namespaceName) => new()
    {
        ApiVersion = $"{_settings.Group}/{_settings.Version}",
        Kind = "Shop",
        Metadata = new V1ObjectMeta
        {
            Name = $"shop-{store.Id}",
            NamespaceProperty = namespaceName,
            Labels = new Dictionary<string, string>
            {
                ["app.kubernetes.io/managed-by"] = "shophub",
                ["shophub.io/store-id"] = store.Id.ToString(),
                ["shophub.io/user-id"] = store.UserId.ToString(),
            }
        },
        Spec = new ShopSpec
        {
            Replicas = (int)store.Availability,
            WalletAddress = store.WalletAddress,
            Database = new ShopDatabase
            {
                Type = store.DatabaseType == DatabaseType.Standard ? "postgresql" : "redis"
            }
        }
    };

    private static async Task EnsureNamespaceAsync(IKubernetes client, string name, CancellationToken cancellationToken)
    {
        try
        {
            await client.CoreV1.ReadNamespaceAsync(name, cancellationToken: cancellationToken);
        }
        catch (k8s.Autorest.HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            await client.CoreV1.CreateNamespaceAsync(
                new V1Namespace { Metadata = new V1ObjectMeta { Name = name } },
                cancellationToken: cancellationToken);
        }
    }

    private static IKubernetes BuildClient()
    {
        var config = KubernetesClientConfiguration.IsInCluster()
            ? KubernetesClientConfiguration.InClusterConfig()
            : KubernetesClientConfiguration.BuildConfigFromConfigFile();

        return new k8s.Kubernetes(config);
    }
}
