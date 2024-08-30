using k8s;
using k8s.Models;
using K8sOperator.NET.Builder;
using K8sOperator.NET.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace K8sOperator.NET;

internal static class ControllerFactory
{
    public static ControllerResult Create(Type controllerType, IServiceProvider serviceProvider, IOperatorBuilder builder)
    {
        var resourceType = controllerType.BaseType!.GetGenericArguments()[0];
        var controller = (IController)ActivatorUtilities.CreateInstance(serviceProvider, controllerType);
        
        return new()
        {
            Controller = controller,
            ResourceType = resourceType,
            Metadata = builder.MetaData
        };
        
    }

}


internal class ControllerResult
{
    public required IController Controller { get;set; }
    public required Type ResourceType { get; set; }
    public required List<object> Metadata { get; set; }
}
