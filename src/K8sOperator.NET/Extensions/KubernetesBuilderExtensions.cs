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
        services.AddTransient<IKubernetes>(x => {
            KubernetesClientConfiguration config;

        if (KubernetesClientConfiguration.IsInCluster())
        {
            config = KubernetesClientConfiguration.InClusterConfig();
        }
        else
        {
            config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
        }

        return services;
    }
}
