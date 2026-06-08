using k8s.Models;

namespace ShopHub.Infrastructure.Kubernetes;

public class RedbCustomResource
{
    public string ApiVersion { get; set; } = "app.redislabs.com/v1alpha1";
    public string Kind { get; set; } = "RedisEnterpriseDatabase";
    public V1ObjectMeta Metadata { get; set; } = new();
    public RedbSpec Spec { get; set; } = new();
}

public class RedbSpec
{
    public string MemorySize { get; set; } = "100MB";
    public string DatabaseSecretName { get; set; } = string.Empty;
}
