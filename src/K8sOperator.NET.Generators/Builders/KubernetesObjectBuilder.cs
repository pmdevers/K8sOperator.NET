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

internal class KubernetesObjectBuilder<T> : IKubernetesObjectBuilder<T>
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
