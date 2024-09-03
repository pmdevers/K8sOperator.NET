using K8sOperator.NET.Models;

namespace K8sOperator.NET;

/// <summary>
/// Represents a controller interface for managing Kubernetes resources.
/// </summary>
public interface IController
{
    /// <summary>
    /// Gets the type of the Kubernetes resource that the controller manages.
    /// </summary>
    Type ResourceType { get; }
}

/// <summary>
/// Represents a base controller for managing Kubernetes custom resources of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of custom resource managed by the controller.</typeparam>
public abstract class Controller<T> : IController
    where T : CustomResource
{
    /// <summary>
    /// Gets the type of the Kubernetes resource managed by the controller.
    /// </summary>
    public Type ResourceType { get; } = typeof(T);

    /// <summary>
    /// Handles adding or modifying the specified resource.
    /// </summary>
    /// <param name="resource">The resource to add or modify.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual Task AddOrModifyAsync(T resource, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles deleting the specified resource.
    /// </summary>
    /// <param name="resource">The resource to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual Task DeleteAsync(T resource, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles finalizing the specified resource.
    /// </summary>
    /// <param name="resource">The resource to finalize.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual Task FinalizeAsync(T resource, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles bookmarking the specified resource for tracking purposes.
    /// </summary>
    /// <param name="resource">The resource to bookmark.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual Task BookmarkAsync(T resource, CancellationToken cancellationToken) 
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles logging or processing an error related to the specified resource.
    /// </summary>
    /// <param name="resource">The resource that encountered an error.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual Task ErrorAsync(T resource, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
