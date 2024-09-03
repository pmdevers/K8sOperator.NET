using k8s;
using k8s.Models;
using K8sOperator.NET.Extensions;
using K8sOperator.NET.Metadata;
using K8sOperator.NET.Models;
using Microsoft.Extensions.Logging;

namespace K8sOperator.NET;

/// <summary>
/// Desccibes an EventWatcher
/// </summary>
public interface IEventWatcher
{
    public IReadOnlyList<object> Metadata { get; }
    public IController Controller { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task Start(CancellationToken cancellationToken);
}

internal class EventWatcher<T>(
    IKubernetes client, 
    Controller<T> controller,
    List<object> metadata,
    ILoggerFactory loggerfactory) : IEventWatcher
    where T: CustomResource
{
    private KubernetesEntityAttribute Crd => metadata.OfType<KubernetesEntityAttribute>().First();
    private string Namespace => metadata.OfType<IWatchNamespaceMetadata>().FirstOrDefault()?.Namespace ?? "default";
    private string LabelSelector => metadata.OfType<ILabelSelectorMetadata>().FirstOrDefault()?.LabelSelector ?? string.Empty;
    private string Finalizer => metadata.OfType<IFinalizerMetadata>().FirstOrDefault()?.Name ?? FinalizerMetadata.Default;

    private readonly ChangeTracker _changeTracker = new();
    private ILogger logger => loggerfactory.CreateLogger("watcher");
    private bool _isRunning;
    private CancellationToken _cancellationToken = CancellationToken.None;
    
    public IReadOnlyList<object> Metadata => metadata;
    public IController Controller => controller;

    public async Task Start(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        _isRunning = true;

        var response = client.CustomObjects.ListNamespacedCustomObjectWithHttpMessagesAsync(
            Crd.Group,
            Crd.ApiVersion,
            Namespace,
            Crd.PluralName,
            watch: true,
            allowWatchBookmarks: true,
            labelSelector: LabelSelector,
            timeoutSeconds: (int)TimeSpan.FromMinutes(60).TotalSeconds,
            cancellationToken: cancellationToken
        );

        logger.BeginWatch(Namespace, Crd.PluralName, LabelSelector);

        await foreach (var (type, item) in response.WatchAsync<T, object>(OnError, cancellationToken))
        {
            OnEvent(type, item);
        }

        logger.EndWatch(Namespace, Crd.PluralName, LabelSelector);
    }

    private void OnEvent(WatchEventType eventType, T customResource)
    {
        logger.EventReceived(eventType, customResource);

        ProccessEventAsync(eventType, customResource!)
            .ContinueWith(t =>
            {
                if(t.IsFaulted)
                {
                    var exception = t.Exception.Flatten().InnerException;
                    logger.ProcessEventError(exception, eventType, customResource);
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
                await HandleBookmarkEventAsync(controller, resource);
                break;
            default:
                break;
        }
    }

    private async Task HandleErrorEventAsync(T resource, CancellationToken cancellationToken)
    {
        logger.HandleError(resource);

        logger.BeginError(resource);

        await controller.ErrorAsync(resource, cancellationToken);
        
        logger.EndError(resource);
    }

    private async Task HandleBookmarkEventAsync(Controller<T> controller, T resource)
    {
        logger.HandleBookmark(resource);

        logger.BeginBookmark(resource);

        await controller.BookmarkAsync(resource, _cancellationToken);
        
        _changeTracker.TrackResourceGenerationAsHandled(resource);

        logger.EndBookmark(resource);
    }

    private async Task HandleFinalizeAsync(T resource, CancellationToken cancellationToken)
    {
        logger.HandleFinalize(resource);

        if (!HasFinalizers(resource))
        {
            logger.SkipFinalize(resource);
            return;
        }

        logger.BeginFinalize(resource);

        await controller.FinalizeAsync(resource, cancellationToken);

        if (HasFinalizers(resource))
        {
            logger.RemoveFinalizer(resource);
            await RemoveFinalizerAsync(resource, cancellationToken);
        }

        logger.EndFinalize(resource);
    }

    private async Task HandleDeletedEventAsync(T resource, CancellationToken cancellationToken)
    {
        logger.HandleDelete(resource);

        if (!HasFinalizers(resource))
        {
            logger.SkipDelete(resource);
            return;
        }

        logger.BeginDelete(resource);

        await controller.DeleteAsync(resource, cancellationToken);

        _changeTracker.TrackResourceGenerationAsDeleted(resource);

        logger.EndDelete(resource);
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
        logger.ReplaceResource(resource);

        // Replace the resource
        var result = await client.CustomObjects.ReplaceNamespacedCustomObjectAsync<T>(
            resource,
            Crd.Group,
            Crd.ApiVersion,
            resource.Metadata.NamespaceProperty,
            Crd.PluralName,
            resource.Metadata.Name,
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);

        return result;
    }

    private bool HasFinalizers(T resource)
        => resource.Metadata.Finalizers?.Contains(Finalizer) == true;

    private async Task HandleAddOrModifyAsync(T resource, CancellationToken cancellationToken)
    {
        logger.HandleAddOrModify(resource);

        if (!HasFinalizers(resource))
        {
            logger.AddFinalizer(resource);
            await AddFinalizerAsync(resource, cancellationToken);
            return;
        }

        if (_changeTracker.IsResourceGenerationAlreadyHandled(resource))
        {
            logger.SkipAddOrModify(resource);
            return;
        }

        await controller.AddOrModifyAsync(resource, cancellationToken);
        _changeTracker.TrackResourceGenerationAsHandled(resource);

        logger.EndAddOrModify(resource);
    }

    private void OnError(Exception exception)
    {
        if (_isRunning)
        {
            logger.LogError(exception, "Watcher error");
        }
    }
}

