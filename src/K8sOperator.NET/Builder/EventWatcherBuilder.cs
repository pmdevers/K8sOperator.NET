using Microsoft.Extensions.DependencyInjection;

namespace K8sOperator.NET.Builder;

internal class EventWatcherBuilder(IServiceProvider serviceProvider, IController controller, IReadOnlyList<object> metadata) 
{
    public IEventWatcher Build()
    {
        var watcherType = typeof(EventWatcher<>).MakeGenericType(controller.ResourceType);
        return (IEventWatcher)ActivatorUtilities.CreateInstance(serviceProvider, watcherType, controller, metadata);
    }
}
