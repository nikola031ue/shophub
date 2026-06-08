using k8s.Models;

namespace ShopHub.Infrastructure.Kubernetes;

public class ShopCustomResource
{
    public string ApiVersion { get; set; } = string.Empty;
    public string Kind { get; set; } = "Shop";
    public V1ObjectMeta Metadata { get; set; } = new();
    public ShopSpec Spec { get; set; } = new();
}

public class ShopSpec
{
    public int Replicas { get; set; }
    public string WalletAddress { get; set; } = string.Empty;
    public ShopDatabase Database { get; set; } = new();
}

public class ShopDatabase
{
    public string Type { get; set; } = string.Empty;
    public ShopDatabaseSecretRef? ConnectionSecretRef { get; set; }
}

public class ShopDatabaseSecretRef
{
    public string Name { get; set; } = string.Empty;
    public string Key { get; set; } = "connectionString";
}
