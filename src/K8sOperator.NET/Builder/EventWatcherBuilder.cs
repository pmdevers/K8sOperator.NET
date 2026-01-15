using Microsoft.Extensions.DependencyInjection;

namespace K8sOperator.NET.Builder;

public class EventWatcherBuilder
{
    public IServiceProvider ServiceProvider { get; }
    public IOperatorController Controller { get; }
    public List<object> Metadata { get; }

    private EventWatcherBuilder(IServiceProvider serviceProvider,
        IOperatorController controller,
        List<object> metadata)
    {
        ServiceProvider = serviceProvider;
        Controller = controller;
        Metadata = metadata;
    }

    public static EventWatcherBuilder Create(IServiceProvider serviceProvider, IOperatorController controller, List<object> metadata)
        => new(serviceProvider, controller, metadata);

    public IEventWatcher Build()
    {
        var watcherType = typeof(EventWatcher<>).MakeGenericType(Controller.ResourceType);

        return (IEventWatcher)ActivatorUtilities.CreateInstance(ServiceProvider, watcherType, Controller, Metadata);
    }
}
