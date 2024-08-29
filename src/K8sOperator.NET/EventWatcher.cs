using k8s;
using k8s.Models;
using K8sOperator.NET.Extensions;
using K8sOperator.NET.Metadata;
using K8sOperator.NET.Models;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace K8sOperator.NET;

/// <summary>
/// Desccibes an EventWatcher
/// </summary>
public interface IEventWatcher
{
    /// <summary>
    /// 
    /// </summary>
    KubernetesEntityAttribute EntityAttribute{get;}
    /// <summary>
    /// 
    /// </summary>
    string Namespace{get;}
    /// <summary>
    /// 
    /// </summary>
    string LabelSelector { get; }

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
    public KubernetesEntityAttribute EntityAttribute => typeof(T).GetCustomAttribute<KubernetesEntityAttribute>()!;
    public string Namespace => metadata.OfType<IWatchNamespaceMetadata>().FirstOrDefault()?.Namespace ?? "default";
    public string LabelSelector => metadata.OfType<ILabelSelectorMetadata>().FirstOrDefault()?.LabelSelector ?? string.Empty;

    private readonly ChangeTracker _changeTracker = new();

    private string Finalizer => metadata.OfType<IFinalizerMetadata>().FirstOrDefault()?.Name ?? FinalizerMetadata.Default;
    private ILogger logger => loggerfactory.CreateLogger("watcher");

    
    private bool _isRunning;
    
    private CancellationToken _cancellationToken = CancellationToken.None;

    public async Task Start(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        _isRunning = true;

        var response = await client.CustomObjects.ListNamespacedCustomObjectWithHttpMessagesAsync(
            EntityAttribute.Group,
            EntityAttribute.ApiVersion,
            Namespace,
            EntityAttribute.PluralName,
            watch: true,
            labelSelector: LabelSelector,
            timeoutSeconds: (int)TimeSpan.FromMinutes(60).TotalSeconds,
            cancellationToken: cancellationToken
        ).ConfigureAwait(false)!;
        logger.BeginWatch(Namespace, EntityAttribute.Kind, LabelSelector);

        using var _ = response.Watch<T, object>(OnEvent, OnError, OnClosed);
        await WaitOneAsync(cancellationToken.WaitHandle);

        logger.EndWatch(Namespace, EntityAttribute.PluralName, LabelSelector);
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
                await HandleAddOrModifyAsync(resource, _cancellationToken);
                break;
            case WatchEventType.Deleted:
                await HandleDeletedEventAsync(resource, _cancellationToken);
                break;
            case WatchEventType.Error:
                break;
            case WatchEventType.Bookmark:
                break;
            default:
                break;
        }
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

        if (HasFinalizers(resource))
        {
            await RemoveFinalizerAsync(resource, cancellationToken);
        }

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
            EntityAttribute.Group,
            EntityAttribute.ApiVersion,
            resource.Metadata.NamespaceProperty,
            EntityAttribute.PluralName,
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


    private void OnClosed()
    {
        logger.LogError("Watcher closed.");

        if (_isRunning)
        {
            throw new InvalidOperationException();
        }
    }

    private void OnError(Exception exception)
    {
        if (_isRunning)
        {
            logger.LogError(exception, "Watcher error");
        }
    }

    private static Task<bool> WaitOneAsync(WaitHandle waitHandle, int millisecondsTimeOutInterval = Timeout.Infinite)
    {
        ArgumentNullException.ThrowIfNull(waitHandle);

        var tcs = new TaskCompletionSource<bool>();

        var rwh = ThreadPool.RegisterWaitForSingleObject(
            waitHandle,
            callBack: (_, timedOut) => { tcs.TrySetResult(!timedOut); },
            state: null,
            millisecondsTimeOutInterval: millisecondsTimeOutInterval,
            executeOnlyOnce: true
        );

        var task = tcs.Task;

        task.ContinueWith(t =>
        {
            rwh.Unregister(waitObject: null);
            try
            {
                return t.Result;
            }
            catch
            {
                return false;
                throw;
            }
        });

        return task;
    }
}

