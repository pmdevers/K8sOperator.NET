using k8s;
using k8s.Models;

namespace K8sOperator.NET.Generator.Builders;

/// <summary>
/// Provides extension methods for configuring Kubernetes objects with metadata.
/// </summary>
public static class KubernetesObjectBuilderExtentions
{
    /// <summary>
    /// Sets the name of the Kubernetes object.
    /// </summary>
    /// <typeparam name="T">The type of the Kubernetes object.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="name">The name to assign to the Kubernetes object.</param>
    /// <returns>The configured builder.</returns>
    public static IKubernetesObjectBuilder<T> WithName<T>(this IKubernetesObjectBuilder<T> builder, string name)
        where T : IMetadata<V1ObjectMeta>
    {
        builder.Add(x => {
            x.Metadata.Name = name;
        });
        return builder;
    }

    /// <summary>
    /// Adds a label to the Kubernetes object.
    /// </summary>
    /// <typeparam name="T">The type of the Kubernetes object.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="key">The key of the label.</param>
    /// <param name="value">The value of the label.</param>
    /// <returns>The configured builder.</returns>
    public static IKubernetesObjectBuilder<T> WithLabel<T>(this IKubernetesObjectBuilder<T> builder, string key, string value)
        where T : IMetadata<V1ObjectMeta>
    {
        builder.Add(x => {
            x.Metadata.Labels ??= new Dictionary<string, string>();
            x.Metadata.Labels.Add(key, value);
        });
        return builder;
    }
}
