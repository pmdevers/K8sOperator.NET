using k8s;
using Microsoft.Extensions.DependencyInjection;

namespace K8sOperator.NET.Extensions;

/// <summary>
/// Extension methods for Kubenetes
/// </summary>
public static class KubernetesBuilderExtensions
{
    /// <summary>
    /// Adds Kubernetes client to the servicecollection
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddKubernetes(this IServiceCollection services)
    {
        KubernetesClientConfiguration config;

        if (KubernetesClientConfiguration.IsInCluster())
        {
            config = KubernetesClientConfiguration.InClusterConfig();
        }
        else
        {
            config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
        }

        services.AddSingleton<IKubernetes>(new Kubernetes(config));

        return services;
    }
}
