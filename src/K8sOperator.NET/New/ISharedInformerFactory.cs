using k8s;
using k8s.Models;
using K8sOperator.NET.Generation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;

namespace K8sOperator.NET.New;

public record WatchEvent<TResource>
{
    public WatchEventType Type { get; set; }
    public TResource Object { get; set; }
}

public interface IInformer<TResource>
{
    IAsyncEnumerable<WatchEvent<TResource>> Events { get; }

    IReadOnlyList<TResource> List();

    TResource? Get(string name, string? ns = null);

    bool HasSynced { get; }
}

public interface ISharedInformerFactory
{
    IInformer<TResource> GetInformer<TResource>(TimeSpan? resyncPeriod = null)
        where TResource : IKubernetesObject;

    Task StartAsync(CancellationToken cancellationToken);
    Task<bool> WaitForCacheSyncAsync(CancellationToken cancellationToken);
}

public interface IResourceCache<TResource>
{
    IReadOnlyList<TResource> List();
    TResource? Get(string name, string? ns = null);

    void Replace(IEnumerable<TResource> items);
    void Apply(WatchEvent<TResource> watcher);
}

internal class ResourceCache<TResource> : IResourceCache<TResource>
    where TResource : IKubernetesObject<V1ObjectMeta>
{
    private readonly ConcurrentDictionary<(string ns, string name), TResource> _items = [];

    public IReadOnlyList<TResource> List() => [.. _items.Values];

    public void Apply(WatchEvent<TResource> e)
    {
        switch (e.Type)
        {
            case WatchEventType.Added:
            case WatchEventType.Modified:
                AddOrUpdate(e.Object);
                break;
            case WatchEventType.Deleted:
                Remove(e.Object);
                break;
        }
    }

    public TResource? Get(string name, string? ns = null)
        => _items.TryGetValue((ns ?? string.Empty, name), out var result)
        ? result
        : default;

    public void Replace(IEnumerable<TResource> items)
    {
        _items.Clear();
        foreach (var item in items)
            AddOrUpdate(item);
    }

    private void AddOrUpdate(TResource obj)
    {
        var key = (obj.Metadata.NamespaceProperty ?? string.Empty, obj.Metadata.Name);
        _items[key] = obj;
    }

    private void Remove(TResource obj)
    {
        var key = (obj.Metadata.NamespaceProperty ?? string.Empty, obj.Metadata.Name);
        _items.TryRemove(key, out _);
    }
}

public interface IInformerInternal
{

}

internal class ResourceInformer<TResource> :
    IInformer<TResource>, IInformerInternal
    where TResource : IKubernetesObject
{
    private readonly IKubernetes _client;
    private readonly IResourceCache<TResource> _cache;
    private readonly Channel<WatchEvent<TResource>> _events;
    private readonly TimeSpan _resyncPeriod;

    private volatile bool _synced;

    public ResourceInformer(
        IKubernetes client,
        IResourceCache<TResource> cache,
        TimeSpan? resyncPeriod = null)
    {
        _client = client;
        _cache = cache;
        _events = Channel.CreateUnbounded<WatchEvent<TResource>>();
        _resyncPeriod = resyncPeriod ?? TimeSpan.FromMinutes(10);
    }

    public bool HasSynced => _synced;

    public IAsyncEnumerable<WatchEvent<TResource>> Events => _events.Reader.ReadAllAsync();

    public TResource? Get(string name, string? ns = null)
        => _cache.Get(name, ns);

    public IReadOnlyList<TResource> List()
        => _cache.List();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var list = await ListResourcesAsync(cancellationToken);
        _cache.Replace(list);
        _synced = true;

        _ = Task.Run(() => WatchLoop(cancellationToken), cancellationToken);
    }

    public Task<bool> WaitForSyncAsync(CancellationToken cancellationToken)
        => Task.FromResult(_synced);

    private async Task<IEnumerable<TResource>> ListResourcesAsync(CancellationToken cancellationToken)
    {
        var response = await _client.CustomObjects.ListClusterCustomObjectAsync<KubernetesList<TResource>>(
            group: "",
            version: "",
            plural: "",
            allowWatchBookmarks: true,
            labelSelector: "",
            cancellationToken: cancellationToken);

        return response.Items;
    }

    private async Task WatchLoop(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var watcher = GetWatchStream(cancellationToken);
            await foreach (var evt in watcher)
            {
                _cache.Apply(evt);
                await _events.Writer.WriteAsync(evt, cancellationToken);
            }
        }
    }

    private async IAsyncEnumerable<WatchEvent<TResource>> GetWatchStream([EnumeratorCancellation]CancellationToken cancellationToken)
    {
        await foreach (var (type, item) in _client.CustomObjects.WatchListClusterCustomObjectAsync(
                group: "",
                version: "",
                plural: "",
                allowWatchBookmarks: true,
                labelSelector: "",
                timeoutSeconds: (int)TimeSpan.FromMinutes(60).TotalSeconds,
                cancellationToken: cancellationToken))
        {
            yield return new()
            {
                Type = type,
                Object = KubernetesJson.Deserialize<TResource>(((JsonElement)item).GetRawText())
            };
        }
    }
}

internal class SharedInformerFactory(IKubernetes client) : ISharedInformerFactory
{
    private readonly IKubernetes _client = client;
    private readonly ConcurrentDictionary<Type, IInformerInternal> _informers = new();

    public IInformer<TResource> GetInformer<TResource>(TimeSpan? resyncPeriod = null) 
        where TResource : IKubernetesObject<V1ObjectMeta>
    {
        var type = typeof(IInformer<TResource>);

        var informer = _informers.GetOrAdd(type, _ =>
        {
            var cache = new ResourceCache<TResource>();
            return new ResourceInformer<TResource>(_client, cache, resyncPeriod)
        });

        return (IInformer<TResource>)informer;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> WaitForCacheSyncAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
