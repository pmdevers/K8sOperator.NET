using Microsoft.Extensions.DependencyInjection;
using System;

namespace K8sOperator.NET.Builder;

/// <summary>
/// 
/// </summary>
public interface IControllerBuilder
{
    /// <summary>
    /// 
    /// </summary>
    List<object> Metadata { get; }
}

internal class ControllerBuilder(IServiceProvider serviceProvider, Type controllerType) : IControllerBuilder
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly Type _controllerType = controllerType;

    public List<object> Metadata { get; } = [];

    public IController Build()
    {
        return (IController)ActivatorUtilities.CreateInstance(_serviceProvider, _controllerType);
    }
}

internal class EventWatcherBuilder(IServiceProvider serviceProvider, IController controller, IReadOnlyList<object> metadata)
{
    public IEventWatcher Build()
    {
        var watcherType = typeof(EventWatcher<>).MakeGenericType(controller.ResourceType);
        return (IEventWatcher)ActivatorUtilities.CreateInstance(serviceProvider, watcherType, controller, metadata);
    }
}
