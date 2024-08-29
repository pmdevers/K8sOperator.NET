using K8sOperator.NET.Models;

namespace K8sOperator.NET;

/// <summary>
/// 
/// </summary>
public interface IController
{
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
sealed class KubernetesFinalizerAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class Controller<T> : IController
    where T : CustomResource
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual Task AddOrModifyAsync(T resource, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual Task DeleteAsync(T resource, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
