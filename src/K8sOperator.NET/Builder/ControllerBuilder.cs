using k8s.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace K8sOperator.NET.Builder;

/// <summary>
/// Interface for building an Operator Controller.
/// </summary>
public interface IControllerBuilder
{
    /// <summary>
    /// Gets the list of metadata associated with the controller.
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
        var controller = (IController)ActivatorUtilities.CreateInstance(_serviceProvider, _controllerType);
        Metadata.AddRange(_controllerType.GetCustomAttributes());

        var attributes = controller.ResourceType.GetCustomAttributes();
        Metadata.AddRange(attributes);
        return controller;
    }
}
