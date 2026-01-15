using k8s;
using k8s.Models;

namespace K8sOperator.NET.Generation;

/// <summary>
/// Provides extension methods for configuring Kubernetes objects with metadata.
/// </summary>
public static class KubernetesObjectBuilderWithMetadataExtentions
{
    /// <summary>
    /// Sets the name of the Kubernetes object.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <typeparam name="T">The type of the Kubernetes object.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="name">The name to assign to the Kubernetes object.</param>
    /// <returns>The configured builder.</returns>
    public static TBuilder WithName<TBuilder, T>(TBuilder builder, string name)
        where TBuilder : IKubernetesObjectBuilderWithMetadata<T>
        where T : IKubernetesObject<V1ObjectMeta>
    {
        builder.Add(x => x.Metadata.Name = name);
        return builder;
    }
}
