using k8s;
using k8s.Autorest;
using k8s.Models;
using K8sOperator.NET.Extensions;
using K8sOperator.NET.Models;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace K8sOperator.NET;

internal interface IKubernetesClient
{
    Task<HttpOperationResponse<object>> ListAsync<T>(string labelSelector, CancellationToken cancellationToken)
        where T : CustomResource;
    Task<T> ReplaceAsync<T>(T resource, CancellationToken cancellationToken)
        where T : CustomResource;
    

}

internal class NamespacedKubernetesClient(IKubernetes client, ILogger<NamespacedKubernetesClient> logger, string ns = "default") : IKubernetesClient
{
    public IKubernetes Client { get; } = client;
    public ILogger Logger { get; } = logger;
    public string Namespace { get; } = ns;

    public Task<HttpOperationResponse<object>> ListAsync<T>(string labelSelector, CancellationToken cancellationToken) where T : CustomResource
    {
        var info = typeof(T).GetCustomAttribute<KubernetesEntityAttribute>()!;

        Logger.BeginWatch(Namespace, info.PluralName, labelSelector);

        var response = Client.CustomObjects.ListNamespacedCustomObjectWithHttpMessagesAsync(
            info.Group,
            info.ApiVersion,
            Namespace,
            info.PluralName,
            watch: true,
            allowWatchBookmarks: true,
            labelSelector: labelSelector,
            timeoutSeconds: (int)TimeSpan.FromMinutes(60).TotalSeconds,
            cancellationToken: cancellationToken
        );

        return response;
    }

    public async Task<T> ReplaceAsync<T>(T resource, CancellationToken cancellationToken)
        where T : CustomResource
    {
        Logger.ReplaceResource(resource);

        var info = resource.GetType().GetCustomAttribute<KubernetesEntityAttribute>()!;

        // Replace the resource
        var result = await Client.CustomObjects.ReplaceNamespacedCustomObjectAsync<T>(
            resource,
            info.Group,
            info.ApiVersion,
            resource.Metadata.NamespaceProperty,
            info.PluralName,
            resource.Metadata.Name,
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);

        return result;
    }
}


internal class ClusterKubernetesClient(IKubernetes client, ILogger<ClusterKubernetesClient> logger) : IKubernetesClient
{
    public IKubernetes Client { get; } = client;
    public ILogger Logger { get; } = logger;

    public async Task<T> ReplaceAsync<T>(T resource, CancellationToken cancellationToken)
        where T : CustomResource
    {
        Logger.ReplaceResource(resource);

        var info = resource.GetType().GetCustomAttribute<KubernetesEntityAttribute>()!;

        // Replace the resource
        var result = await Client.CustomObjects.ReplaceClusterCustomObjectAsync<T>(
            resource,
            info.Group,
            info.ApiVersion,
            info.PluralName,
            resource.Metadata.Name,
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);

        return result;
    }

    public Task<HttpOperationResponse<object>> ListAsync<T>(string labelSelector, CancellationToken cancellationToken)
        where T : CustomResource
    {
        var info = typeof(T).GetCustomAttribute<KubernetesEntityAttribute>()!;

        Logger.BeginWatch("cluster-wide", info.PluralName, labelSelector);

        var response = Client.CustomObjects.ListClusterCustomObjectWithHttpMessagesAsync(
            info.Group,
            info.ApiVersion,
            info.PluralName,
            watch: true,
            allowWatchBookmarks: true,
            labelSelector: labelSelector,
            timeoutSeconds: (int)TimeSpan.FromMinutes(60).TotalSeconds,
            cancellationToken: cancellationToken
        );

        return response;
    }
}
