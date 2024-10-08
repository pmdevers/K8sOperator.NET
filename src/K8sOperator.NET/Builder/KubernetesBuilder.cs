﻿using Microsoft.Extensions.DependencyInjection;

namespace K8sOperator.NET.Builder;

/// <summary>
/// Describes a IKubernetesBuilder
/// </summary>
public interface IKubernetesBuilder
{
    /// <summary>
    /// The service collection.
    /// </summary>
    public IServiceCollection Services { get; }
}

internal sealed class KubernetesBuilder(IServiceCollection services) : IKubernetesBuilder
{
    public IServiceCollection Services { get; } = services;
}
