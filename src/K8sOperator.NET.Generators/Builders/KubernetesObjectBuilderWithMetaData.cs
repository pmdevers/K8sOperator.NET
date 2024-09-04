using k8s;
using k8s.Models;

namespace K8sOperator.NET.Generators.Builders;

/// <summary>
/// Represents a builder interface for Kubernetes objects that include metadata.
/// </summary>
/// <typeparam name="T">The type of the Kubernetes object that includes metadata.</typeparam>
public interface IKubernetesObjectBuilderWithMetadata<out T> : IKubernetesObjectBuilder<T>
    where T : IMetadata<V1ObjectMeta>
{

}

internal class KubernetesObjectBuilderWithMetaData<T>
    : KubernetesObjectBuilder<T>, IKubernetesObjectBuilderWithMetadata<T>
    where T : class, IMetadata<V1ObjectMeta>, new()
{
    public KubernetesObjectBuilderWithMetaData()
    {
        Add(x => x.Metadata = new V1ObjectMeta());
    }
}
