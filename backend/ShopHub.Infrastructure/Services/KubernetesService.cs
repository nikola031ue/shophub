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

    // CNPG names the app-user secret as "{cluster-name}-app"
    private static string CnpgClusterName(Guid storeId) => $"shop-{storeId}-db";
    private static string CnpgSecretName(Guid storeId) => $"{CnpgClusterName(storeId)}-app";

    public async Task CreateDatabaseAsync(Store store, CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled)
        {
            logger.LogInformation("Kubernetes integration disabled. Skipping CNPG Cluster creation for store {StoreId}.", store.Id);
            return;
        }

        if (store.DatabaseType != DatabaseType.Standard)
        {
            logger.LogInformation("Store {StoreId} uses non-PostgreSQL database. Skipping CNPG Cluster creation.", store.Id);
            return;
        }

        try
        {
            var client = BuildClient();
            var namespaceName = BuildNamespace(store.UserId);

            await EnsureNamespaceAsync(client, namespaceName, cancellationToken);

            var cluster = BuildCnpgCluster(store, namespaceName);
            await client.CustomObjects.CreateNamespacedCustomObjectAsync(
                cluster,
                _settings.Cnpg.Group,
                _settings.Cnpg.Version,
                namespaceName,
                _settings.Cnpg.Plural,
                cancellationToken: cancellationToken);

            logger.LogInformation(
                "Created CNPG Cluster {Name} in namespace {Namespace} for store {StoreId}.",
                cluster.Metadata.Name, namespaceName, store.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create CNPG Cluster for store {StoreId}.", store.Id);
        }
    }

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

    private CnpgClusterCustomResource BuildCnpgCluster(Store store, string namespaceName) => new()
    {
        ApiVersion = $"{_settings.Cnpg.Group}/{_settings.Cnpg.Version}",
        Kind = "Cluster",
        Metadata = new V1ObjectMeta
        {
            Name = CnpgClusterName(store.Id),
            NamespaceProperty = namespaceName,
            Labels = new Dictionary<string, string>
            {
                ["app.kubernetes.io/managed-by"] = "shophub",
                ["shophub.io/store-id"] = store.Id.ToString(),
                ["shophub.io/user-id"] = store.UserId.ToString(),
            }
        },
        Spec = new CnpgClusterSpec
        {
            Instances = _settings.Cnpg.Instances,
            Storage = new CnpgClusterStorage { Size = _settings.Cnpg.StorageSize }
        }
    };

    private ShopCustomResource BuildShopCr(Store store, string namespaceName)
    {
        var database = new ShopDatabase
        {
            Type = store.DatabaseType == DatabaseType.Standard ? "postgresql" : "redis"
        };

        if (store.DatabaseType == DatabaseType.Standard)
        {
            database.ConnectionSecretRef = new ShopDatabaseSecretRef
            {
                Name = CnpgSecretName(store.Id),
                Key = "connectionString"
            };
        }

        return new ShopCustomResource
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
                Database = database
            }
        };
    }

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
