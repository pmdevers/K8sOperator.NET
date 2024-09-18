using K8sOperator.NET.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace K8sOperator.NET.Builder;

internal class EventWatcherBuilder(IServiceProvider serviceProvider, IController controller, IReadOnlyList<object> metadata) 
{
    public IEventWatcher Build()
    {
        var watcherType = typeof(EventWatcher<>).MakeGenericType(controller.ResourceType);
        IKubernetesClient client = ActivatorUtilities.CreateInstance<ClusterKubernetesClient>(serviceProvider);

        var ns = metadata.OfType<INamespaceMetadata>().FirstOrDefault();
        if (ns is not null)
        { 
            client = ActivatorUtilities.CreateInstance<NamespacedKubernetesClient>(serviceProvider);
        }

        
        return (IEventWatcher)ActivatorUtilities.CreateInstance(serviceProvider, watcherType, client, controller, metadata);
    }
}
