using K8sOperator.NET.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace K8sOperator.NET.Builder;

public class ControllerBuilder
{
    private ControllerBuilder(IServiceProvider serviceProvider, Type controllerType, OperatorConfiguration configuration)
    {
        ServiceProvider = serviceProvider;
        ControllerType = controllerType;
        Configuration = configuration;
    }
    public IServiceProvider ServiceProvider { get; }
    public Type ControllerType { get; set; }
    public OperatorConfiguration Configuration { get; }

    public static ControllerBuilder Create(IServiceProvider serviceProvider, Type controllerType, OperatorConfiguration configuration)
        => new(serviceProvider, controllerType, configuration);

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





