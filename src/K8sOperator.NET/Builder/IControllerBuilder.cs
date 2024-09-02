﻿using k8s.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

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
        var controller = (IController)ActivatorUtilities.CreateInstance(_serviceProvider, _controllerType);
        var attribute = controller.ResourceType.GetCustomAttribute<KubernetesEntityAttribute>();

        if (attribute is not null)
        {
            Metadata.Add(attribute);
        }

        return controller;
    }
}

public class ControllerBuilderResult
{
    public IController Controller { get; set; }
    public List<object> Metadata { get; set;}
}

internal class EventWatcherBuilder(IServiceProvider serviceProvider, IController controller, IReadOnlyList<object> metadata)
{
    public IEventWatcher Build()
    {
        var watcherType = typeof(EventWatcher<>).MakeGenericType(controller.ResourceType);
        return (IEventWatcher)ActivatorUtilities.CreateInstance(serviceProvider, watcherType, controller, metadata);
    }
}
