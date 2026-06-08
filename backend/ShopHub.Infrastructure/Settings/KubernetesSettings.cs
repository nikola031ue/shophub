namespace ShopHub.Infrastructure.Settings;

public class KubernetesSettings
{
    public const string SectionName = "Kubernetes";

    public bool Enabled { get; init; } = true;
    public string NamespacePrefix { get; init; } = "shops";
    public string Group { get; init; } = "shop.shophub.io";
    public string Version { get; init; } = "v1alpha1";
    public string Plural { get; init; } = "shops";
    public CnpgSettings Cnpg { get; init; } = new();
    public RedbSettings Redb { get; init; } = new();
}

public class CnpgSettings
{
    public string Group { get; init; } = "postgresql.cnpg.io";
    public string Version { get; init; } = "v1";
    public string Plural { get; init; } = "clusters";
    public string StorageSize { get; init; } = "1Gi";
    public int Instances { get; init; } = 1;
}

public class RedbSettings
{
    public string Group { get; init; } = "app.redislabs.com";
    public string Version { get; init; } = "v1alpha1";
    public string Plural { get; init; } = "redisenterprisedatabases";
    public string MemorySize { get; init; } = "100MB";
}
