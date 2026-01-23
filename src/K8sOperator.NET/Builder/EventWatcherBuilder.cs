using K8sOperator.NET.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace K8sOperator.NET.Builder;

public class EventWatcherBuilder
{
    public IServiceProvider ServiceProvider { get; }
    public IOperatorController Controller { get; }
    public OperatorConfiguration Configuration { get; }
    public List<object> Metadata { get; }

    private EventWatcherBuilder(IServiceProvider serviceProvider,
        OperatorConfiguration configuration,
        IOperatorController controller,
        List<object> metadata)
    {
        ServiceProvider = serviceProvider;
        Configuration = configuration;
        Controller = controller;
        Metadata = metadata;
    }

    public static EventWatcherBuilder Create(IServiceProvider serviceProvider, OperatorConfiguration configuration, IOperatorController controller, List<object> metadata)
        => new(serviceProvider, configuration, controller, metadata);

    public IEventWatcher Build()
    {
        var watcherType = typeof(EventWatcher<>).MakeGenericType(Controller.ResourceType);

        return (IEventWatcher)ActivatorUtilities.CreateInstance(ServiceProvider, watcherType, Configuration, Controller, Metadata);
    }
}
