using k8s;
using k8s.Models;
using K8sOperator.NET.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace K8sOperator.NET;

internal static class EventWatcherFactory
{
    public static IEventWatcher Create(Type controllerType, IServiceProvider serviceProvider, List<object> metadata)
    {
        var resourceType = controllerType.BaseType!.GetGenericArguments()[0];
        var controller = ActivatorUtilities.CreateInstance(serviceProvider, controllerType);
        var watcherType = typeof(EventWatcher<>).MakeGenericType(resourceType);

        return (IEventWatcher)ActivatorUtilities.CreateInstance(serviceProvider, watcherType, controller, metadata);
        
    }

}
