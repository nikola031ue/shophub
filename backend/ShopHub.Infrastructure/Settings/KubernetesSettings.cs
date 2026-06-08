namespace ShopHub.Infrastructure.Settings;

public class KubernetesSettings
{
    public const string SectionName = "Kubernetes";

    public bool Enabled { get; init; } = true;
    public string NamespacePrefix { get; init; } = "shops";
    public string Group { get; init; } = "shop.shophub.io";
    public string Version { get; init; } = "v1alpha1";
    public string Plural { get; init; } = "shops";
}
