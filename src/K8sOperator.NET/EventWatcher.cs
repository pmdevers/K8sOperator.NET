using k8s;
using k8s.Autorest;
using k8s.Models;
using K8sOperator.NET.Extensions;
using K8sOperator.NET.Metadata;
using K8sOperator.NET.Models;
using Microsoft.Extensions.Logging;

namespace K8sOperator.NET;

/// <summary>
/// Interface representing an event watcher that monitors Kubernetes events and interacts with a controller.
/// </summary>
public interface IEventWatcher
{
    /// <summary>
    /// Gets the metadata associated with the event watcher.
    /// </summary>
    public IReadOnlyList<object> Metadata { get; }

    /// <summary>
    /// Gets the controller that processes events captured by the event watcher.
    /// </summary>
    public IController Controller { get; }

    /// <summary>
    /// Starts the event watcher, monitoring for events and processing them using the controller.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Start(CancellationToken cancellationToken);
}

internal class EventWatcher<T>(IKubernetesClient client, Controller<T> controller, List<object> metadata, ILoggerFactory loggerfactory) : IEventWatcher
    where T: CustomResource
{
    private KubernetesEntityAttribute Crd => Metadata.OfType<KubernetesEntityAttribute>().First();
    private string LabelSelector => Metadata.OfType<ILabelSelectorMetadata>().FirstOrDefault()?.LabelSelector ?? string.Empty;
    private string Finalizer => Metadata.OfType<IFinalizerMetadata>().FirstOrDefault()?.Finalizer ?? FinalizerAttribute.Default;
    
    private readonly ChangeTracker _changeTracker = new();
    private bool _isRunning;
    private CancellationToken _cancellationToken = CancellationToken.None;
    private readonly Controller<T> _controller = controller;

    public IKubernetesClient Client { get; } = client;
    public ILogger Logger { get; } = loggerfactory.CreateLogger("watcher");
    public IReadOnlyList<object> Metadata { get; } = metadata;
    public IController Controller => _controller;

    public async Task Start(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        _isRunning = true;

        var response = Client.ListAsync<T>(LabelSelector, cancellationToken);

        await foreach (var (type, item) in response.WatchAsync<T, object>(OnError, cancellationToken))
        {
            OnEvent(type, item);
        }

        Logger.EndWatch(Crd.PluralName, LabelSelector);
    }
    

    private void OnEvent(WatchEventType eventType, T customResource)
    {
        Logger.EventReceived(eventType, customResource);

        ProccessEventAsync(eventType, customResource!)
            .ContinueWith(t =>
            {
                if(t.IsFaulted)
                {
                    var exception = t.Exception.Flatten().InnerException;
                    Logger.ProcessEventError(exception, eventType, customResource);
                }
            })
            ;
    }

    private async Task ProccessEventAsync(WatchEventType eventType, T resource)
    {
        switch (eventType)
        {
            case WatchEventType.Added:
            case WatchEventType.Modified:
                if(resource.Metadata.DeletionTimestamp is not null)
                {
                    await HandleFinalizeAsync(resource, _cancellationToken);
                } 
                else
                {
                    await HandleAddOrModifyAsync(resource, _cancellationToken);
                }
                break;
            case WatchEventType.Deleted:
                await HandleDeletedEventAsync(resource, _cancellationToken);
                break;
            case WatchEventType.Error:
                await HandleErrorEventAsync(resource, _cancellationToken);
                break;
            case WatchEventType.Bookmark:
                await HandleBookmarkEventAsync(resource, _cancellationToken);
                break;
            default:
                break;
        }
    }

    private async Task HandleErrorEventAsync(T resource, CancellationToken cancellationToken)
    {
        Logger.HandleError(resource);

        Logger.BeginError(resource);

        await _controller.ErrorAsync(resource, cancellationToken);

        Logger.EndError(resource);
    }

    private async Task HandleBookmarkEventAsync(T resource, CancellationToken cancellationToken)
    {
        Logger.HandleBookmark(resource);

        Logger.BeginBookmark(resource);

        await _controller.BookmarkAsync(resource, cancellationToken);
        
        _changeTracker.TrackResourceGenerationAsHandled(resource);

        Logger.EndBookmark(resource);
    }

    private async Task HandleFinalizeAsync(T resource, CancellationToken cancellationToken)
    {
        Logger.HandleFinalize(resource);

        if (!HasFinalizers(resource))
        {
            Logger.SkipFinalize(resource);
            return;
        }

        Logger.BeginFinalize(resource);

        await _controller.FinalizeAsync(resource, cancellationToken);

        if (HasFinalizers(resource))
        {
            Logger.RemoveFinalizer(resource);
            await RemoveFinalizerAsync(resource, cancellationToken);
        }

        Logger.EndFinalize(resource);
    }

    private async Task HandleDeletedEventAsync(T resource, CancellationToken cancellationToken)
    {
        Logger.HandleDelete(resource);

        if (!HasFinalizers(resource))
        {
            Logger.SkipDelete(resource);
            return;
        }

        Logger.BeginDelete(resource);

        await _controller.DeleteAsync(resource, cancellationToken);

        _changeTracker.TrackResourceGenerationAsDeleted(resource);

        Logger.EndDelete(resource);
    }

    private Task<T> RemoveFinalizerAsync(T resource, CancellationToken cancellationToken)
    {
        resource.Metadata.Finalizers.Remove(Finalizer);
        return ReplaceAsync(resource, cancellationToken);
    }

    private Task<T> AddFinalizerAsync(T resource, CancellationToken cancellationToken)
    {
        // Add the finalizer
        resource.Metadata.EnsureFinalizers().Add(Finalizer);

        return ReplaceAsync(resource, cancellationToken);
    }


    private async Task<T> ReplaceAsync(T resource, CancellationToken cancellationToken)
    {
        Logger.ReplaceResource(resource);

        // Replace the resource
        var result = await Client.ReplaceAsync(resource, cancellationToken);

        return result;
    }

    private bool HasFinalizers(T resource)
        => resource.Metadata.Finalizers?.Contains(Finalizer) ?? false;

    private async Task HandleAddOrModifyAsync(T resource, CancellationToken cancellationToken)
    {
        Logger.HandleAddOrModify(resource);

        if (!HasFinalizers(resource))
        {
            Logger.AddFinalizer(resource);
            await AddFinalizerAsync(resource, cancellationToken);
            return;
        }

        if (_changeTracker.IsResourceGenerationAlreadyHandled(resource))
        {
            Logger.SkipAddOrModify(resource);
            return;
        }

        await _controller.AddOrModifyAsync(resource, cancellationToken);
        
        resource = await ReplaceAsync(resource, _cancellationToken);
        
        _changeTracker.TrackResourceGenerationAsHandled(resource);

        Logger.EndAddOrModify(resource);
    }

    private void OnError(Exception exception)
    {
        if (_isRunning)
        {
            Logger.LogError(exception, "Watcher error");
        }
    }
}

