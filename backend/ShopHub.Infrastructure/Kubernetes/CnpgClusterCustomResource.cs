using k8s.Models;

namespace ShopHub.Infrastructure.Kubernetes;

public class CnpgClusterCustomResource
{
    public string ApiVersion { get; set; } = "postgresql.cnpg.io/v1";
    public string Kind { get; set; } = "Cluster";
    public V1ObjectMeta Metadata { get; set; } = new();
    public CnpgClusterSpec Spec { get; set; } = new();
}

public class CnpgClusterSpec
{
    public int Instances { get; set; } = 1;
    public CnpgClusterStorage Storage { get; set; } = new();
}

public class CnpgClusterStorage
{
    public string Size { get; set; } = "1Gi";
}
