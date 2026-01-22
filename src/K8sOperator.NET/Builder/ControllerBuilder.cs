using Microsoft.Extensions.DependencyInjection;

namespace K8sOperator.NET.Builder;

public class ControllerBuilder
{
    private ControllerBuilder(IServiceProvider serviceProvider, Type controllerType, List<object> metadata)
    {
        ServiceProvider = serviceProvider;
        ControllerType = controllerType;
        Metadata = metadata;
    }
    public IServiceProvider ServiceProvider { get; }
    public Type ControllerType { get; set; }

    public static ControllerBuilder Create(IServiceProvider serviceProvider, Type controllerType, List<object> metadata)
        => new(serviceProvider, controllerType, metadata);

    public IOperatorController Build()
    {
        var controller = (IOperatorController)ActivatorUtilities.CreateInstance(ServiceProvider, ControllerType);
        Metadata.AddRange(ControllerType.GetCustomAttributes(true));

        var attributes = controller.ResourceType.GetCustomAttributes(true);
        Metadata.AddRange(attributes);

        return controller;
    }
    public List<object> Metadata { get; } = [];
}


