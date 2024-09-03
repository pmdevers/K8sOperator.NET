using k8s;
using k8s.Models;

namespace K8sOperator.NET.Generator.Builders;

/// <summary>
/// Describes a Generic Kubernetes Resource Builder
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IKubernetesObjectBuilder<out T>
{
    /// <summary>
    /// Adds an action to the builder.
    /// </summary>
    /// <param name="action"></param>
    void Add(Action<T> action);

    /// <summary>
    /// Builds the resource with the added actions.
    /// </summary>
    /// <returns></returns>
    T Build();
}

/// <summary>
/// Implementation of a generic KubernetesObjectBuilder
/// </summary>
/// <typeparam name="T">The kubernetes object type.</typeparam>
public class KubernetesObjectBuilder<T> : IKubernetesObjectBuilder<T>
    where T : class, new()
{
    private readonly List<Action<T>> _actions = [];

    public void Add(Action<T> action)
    {
        _actions.Add(action);
    }

    public virtual T Build()
    {
        var o = new T();

        foreach (var action in _actions)
        {
            action(o);
        }

        return o;
    }
}

public interface IKubernetesObjectBuilderWithMetadata<T> : IKubernetesObjectBuilder<T>
    where T : IMetadata<V1ObjectMeta>
{

}

/// <summary>
/// Implementation of a generic KubernetesObjectBuilder
/// </summary>
/// <typeparam name="T">The kubernetes object type.</typeparam>
public class KubernetesObjectBuilderWithMetaData<T> 
    : KubernetesObjectBuilder<T>, IKubernetesObjectBuilderWithMetadata<T>
    where T : class, IMetadata<V1ObjectMeta>, new()
{
    public KubernetesObjectBuilderWithMetaData()
    {
        Add(x=>x.Metadata = new V1ObjectMeta());
    }
}

public static class KubernetesObjectBuilderWithMetadataExtentions
{
    public static TBuilder WithName<TBuilder, T>(TBuilder builder, string name)
        where TBuilder : IKubernetesObjectBuilderWithMetadata<T>
        where T : IKubernetesObject<V1ObjectMeta>
    {
        builder.Add(x => x.Metadata.Name = name);
        return builder;
    }
}
