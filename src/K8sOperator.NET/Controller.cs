using K8sOperator.NET.Models;

namespace K8sOperator.NET;

/// <summary>
/// 
/// </summary>
public interface IController
{
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual Task FinalizeAsync(T resource, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual Task BookmarkAsync(T resource, CancellationToken cancellationToken) 
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual Task ErrorAsync(T resource, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
