using k8s;
using K8sOperator.NET.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace K8sOperator.NET.Extensions;
/// <summary>
/// 
/// </summary>
public static class KubernetesBuilderExtensions
{
    /// <summary>
    /// 
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
