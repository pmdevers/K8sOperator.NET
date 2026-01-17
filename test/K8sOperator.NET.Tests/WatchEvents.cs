using k8s;

namespace K8sOperator.NET.Tests;

public static class WatchEvents<T>
    where T : CustomResource, new()
{
    public static Watcher<T>.WatchEvent Added => CreateEvent(WatchEventType.Modified,
        new T()
        {
            Metadata = new()
            {
                Name = "test",
                NamespaceProperty = "default",
                Finalizers = ["finalize"],
                Uid = "1"
            }
        });

    public static Watcher<T>.WatchEvent Finalize => CreateEvent(WatchEventType.Added,
            new T()
            {
                Metadata = new()
                {
                    Name = "test",
                    NamespaceProperty = "default",
                    DeletionTimestamp = TimeProvider.System.GetUtcNow().DateTime,
                    Finalizers = ["finalize"],
                    Uid = "1"
                }
            });

    public static Watcher<T>.WatchEvent Deleted => CreateEvent(WatchEventType.Deleted,
            new T()
            {
                Metadata = new()
                {
                    Name = "test",
                    NamespaceProperty = "default",
                    Finalizers = ["finalize"],
                    Uid = "1"
                }
            });

    public static Watcher<T>.WatchEvent CreateEvent(WatchEventType type, T item)
    {
        return new Watcher<T>.WatchEvent { Type = type, Object = item };
    }
}
