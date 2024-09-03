using k8s;
using k8s.Models;

namespace K8sOperator.NET.Generator.Builders;

public static class KubernetesObjectBuilderExtentions
{
    public static IKubernetesObjectBuilder<T> WithName<T>(this IKubernetesObjectBuilder<T> builder, string name)
        where T : IMetadata<V1ObjectMeta>
    {
        builder.Add(x => {
            x.Metadata.Name = name;
        });
        return builder;
    }

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
