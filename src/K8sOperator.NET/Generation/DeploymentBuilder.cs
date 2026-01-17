using k8s;
using k8s.Models;

namespace K8sOperator.NET.Generation;

/// <summary>
/// Provides functionality for creating Kubernetes Deployment objects.
/// </summary>
public static class DeploymentBuilder
{
    /// <summary>
    /// Creates a new instance of a deployment builder that includes metadata configuration.
    /// </summary>
    /// <returns>An instance of <see cref="IKubernetesObjectBuilderWithMetadata{V1Deployment}"/> for building a Kubernetes Deployment.</returns>
    public static IKubernetesObjectBuilderWithMetadata<V1Deployment> Create()
        => new DeploymentBuilderImp();
}

internal class DeploymentBuilderImp : KubernetesObjectBuilderWithMetadata<V1Deployment>
{
    public override V1Deployment Build()
    {
        var deployment = base.Build();
        deployment.Initialize();
        return deployment;
    }
}
