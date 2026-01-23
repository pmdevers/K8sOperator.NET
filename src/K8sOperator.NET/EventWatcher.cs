using k8s;
using k8s.Autorest;
using k8s.Models;
using K8sOperator.NET;
using K8sOperator.NET.Configuration;
using K8sOperator.NET.Generation;
using K8sOperator.NET.Metadata;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace K8sOperator.NET;

public class EventWatcher<T>(
    OperatorConfiguration configuration,
    IKubernetes kubernetes,
    OperatorController<T> controller,
    List<object> metadata,
    ILoggerFactory loggerFactory) : IEventWatcher
    where T : CustomResource
{

    public OperatorConfiguration Configuration { get; } = configuration;
    public IReadOnlyList<object> Metadata { get; } = metadata;
    public ILogger Logger { get; } = loggerFactory.CreateLogger("Watcher");
    public IOperatorController Controller { get; } = controller;

    public async Task Start(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        _isRunning = true;

        while (_isRunning && !_cancellationToken.IsCancellationRequested)
        {
            try
            {
                Logger.BeginWatch(Crd.PluralName, LabelSelector.LabelSelector);

                await foreach (var (type, item) in GetWatchStream())
                {
                    if (item is JsonElement je)
                    {
                        var i = KubernetesJson.Deserialize<T>(je);
                        if (i is not null)
                        {
                            OnEvent(type, i);
                            continue;
                        }
                    }
                    else if (item is T resource)
                    {
                        OnEvent(type, resource);
                        continue;
                    }// Handle each watch event
                }
            }
            catch (TaskCanceledException ex)
            {
                Logger.WatcherError("Task was canceled: " + ex.Message);
            }
            catch (OperationCanceledException ex)
            {
                Logger.WatcherError("Operation was canceled: " + ex.Message);
            }
            catch (HttpOperationException ex)
            {
                Logger.WatcherError("Http Error: " + ex.Response.Content);
            }
            catch (HttpRequestException ex)
            {
                Logger.WatcherError("Http Request Error: " + ex.Message);
            }
            finally
            {
                Logger.EndWatch(Crd.PluralName, LabelSelector.LabelSelector);

                if (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        Logger.LogInformation("Watcher stopped, waiting to restart...");
                        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                    }
                    catch (TaskCanceledException)
                    {
                        // Ignore cancellation during delay
                    }
                }
            }
        }
    }

    private void OnEvent(WatchEventType eventType, T customResource)
    {
        Logger.EventReceived(eventType, customResource);

        ProccessEventAsync(eventType, customResource!)
            .ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    var exception = t.Exception.Flatten().InnerException;
                    Logger.ProcessEventError(exception, eventType, customResource);
                }
            });
    }

    private async Task ProccessEventAsync(WatchEventType eventType, T resource)
    {
        switch (eventType)
        {
            case WatchEventType.Added:
            case WatchEventType.Modified:
                if (resource.Metadata.DeletionTimestamp is not null)
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

        await controller.ErrorAsync(resource, cancellationToken);

        Logger.EndError(resource);
    }

    private async Task HandleBookmarkEventAsync(T resource, CancellationToken cancellationToken)
    {
        Logger.HandleBookmark(resource);

        Logger.BeginBookmark(resource);

        await controller.BookmarkAsync(resource, cancellationToken);

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

        await controller.FinalizeAsync(resource, cancellationToken);

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

        await controller.DeleteAsync(resource, cancellationToken);

        _changeTracker.TrackResourceGenerationAsDeleted(resource);

        Logger.EndDelete(resource);
    }

    private Task<T> RemoveFinalizerAsync(T resource, CancellationToken cancellationToken)
    {
        resource.Metadata.Finalizers.Remove(Finalizer.Finalizer);
        return ReplaceAsync(resource, cancellationToken);
    }

    private Task<T> AddFinalizerAsync(T resource, CancellationToken cancellationToken)
    {
        // Add the finalizer
        resource.Metadata.EnsureFinalizers().Add(Finalizer.Finalizer);

        return ReplaceAsync(resource, cancellationToken);
    }


    private async Task<T> ReplaceAsync(T resource, CancellationToken cancellationToken)
    {
        Logger.ReplaceResource(resource);

        // Replace the resource
        var result = await ResourceReplaceAsync(resource, cancellationToken);

        return result;
    }

    private bool HasFinalizers(T resource)
        => resource.Metadata.Finalizers?.Contains(Finalizer.Finalizer) ?? false;

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

        await controller.AddOrModifyAsync(resource, cancellationToken);

        resource = await ReplaceAsync(resource, _cancellationToken);

        _changeTracker.TrackResourceGenerationAsHandled(resource);

        Logger.EndAddOrModify(resource);
    }

    private Task<T> ResourceReplaceAsync(T resource, CancellationToken cancellationToken)
    {
        return Scope.Scope switch
        {
            EntityScope.Cluster => kubernetes.CustomObjects.ReplaceClusterCustomObjectAsync<T>(
                body: resource,
                group: Crd.Group,
                version: Crd.ApiVersion,
                plural: Crd.PluralName,
                name: resource.Metadata.Name,
                cancellationToken: cancellationToken),

            EntityScope.Namespaced => kubernetes.CustomObjects.ReplaceNamespacedCustomObjectAsync<T>(
                body: resource,
                group: Crd.Group,
                version: Crd.ApiVersion,
                namespaceParameter: Configuration.Namespace,
                plural: Crd.PluralName,
                name: resource.Metadata.Name,
                cancellationToken: cancellationToken),

            _ => throw new ArgumentException("Invalid scope"),
        };
    }

    private IAsyncEnumerable<(WatchEventType type, object item)> GetWatchStream()
    {
        return Scope.Scope switch
        {
            EntityScope.Cluster => kubernetes.CustomObjects.WatchListClusterCustomObjectAsync(
                group: Crd.Group,
                version: Crd.ApiVersion,
                plural: Crd.PluralName,
                allowWatchBookmarks: true,
                labelSelector: LabelSelector.LabelSelector,
                timeoutSeconds: (int)TimeSpan.FromMinutes(60).TotalSeconds,
                onError: (ex) =>
                {
                    Logger.LogWatchError(ex, "cluster-wide", Crd.PluralName, LabelSelector.LabelSelector);
                },
                cancellationToken: _cancellationToken),

            EntityScope.Namespaced => kubernetes.CustomObjects.WatchListNamespacedCustomObjectAsync(
                group: Crd.Group,
                version: Crd.ApiVersion,
                namespaceParameter: Configuration.Namespace,
                plural: Crd.PluralName,
                allowWatchBookmarks: true,
                labelSelector: LabelSelector.LabelSelector,
                timeoutSeconds: (int)TimeSpan.FromMinutes(60).TotalSeconds,
                onError: (ex) =>
                {
                    Logger.LogWatchError(ex, Configuration.Namespace, Crd.PluralName, LabelSelector.LabelSelector);
                },
                cancellationToken: _cancellationToken),

            _ => throw new ArgumentException("Invalid scope"),
        };
    }

    private CancellationToken _cancellationToken = CancellationToken.None;
    private bool _isRunning = false;

    private readonly ChangeTracker _changeTracker = new();

    private KubernetesEntityAttribute Crd => Metadata.OfType<KubernetesEntityAttribute>().FirstOrDefault()
        ?? throw new InvalidOperationException($"Controller metadata must include a {nameof(KubernetesEntityAttribute)}. Ensure the controller's resource type is properly decorated.");

    private ScopeAttribute Scope => Metadata.OfType<ScopeAttribute>().FirstOrDefault() ??
            ScopeAttribute.Default;
    private FinalizerAttribute Finalizer => Metadata.OfType<FinalizerAttribute>().FirstOrDefault()
        ?? FinalizerAttribute.Default;

    private LabelSelectorAttribute LabelSelector => Metadata.OfType<LabelSelectorAttribute>().FirstOrDefault()
        ?? LabelSelectorAttribute.Default;
}
