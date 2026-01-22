using k8s;
using k8s.Models;

namespace K8sOperator.NET.Generation;

/// <summary>
/// Describes a Generic Kubernetes Resource Builder
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IObjectBuilder<out T>
{
    /// <summary>
    /// Adds an action to the builder.
    /// </summary>
    /// <param name="action"></param>
    IObjectBuilder<T> Add(Action<T> action);

    /// <summary>
    /// Builds the resource with the added actions.
    /// </summary>
    /// <returns></returns>
    T Build();
}

internal class ObjectBuilder<T> : IObjectBuilder<T>
    where T : new()
{
    private readonly List<Action<T>> _actions = [];

    public static IObjectBuilder<T> Create() => new ObjectBuilder<T>();

    public IObjectBuilder<T> Add(Action<T> action)
    {
        _actions.Add(action);
        return this;
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

public static class KubernetesObjectBuilder
{
    /// <summary>
    /// Creates a new Kubernetes object builder for the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the Kubernetes object.</typeparam>
    /// <returns>A new instance of <see cref="IObjectBuilder{T}"/>.</returns>
    public static IObjectBuilder<T> Create<T>()
        where T : IKubernetesObject, new()
    {
        return new ObjectBuilder<T>().Add(x => x.Initialize());
    }

    public static IObjectBuilder<T> CreateMeta<T>()
        where T : IMetadata<V1ObjectMeta>, new()
    {
        return new ObjectBuilder<T>().Add(x => x.Metadata = new());
    }
}
