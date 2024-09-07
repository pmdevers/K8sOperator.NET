using k8s;
using K8sOperator.NET.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace K8sOperator.NET.Tests.Mocks.Endpoints;
internal static class ListNamespacedCustomObjectWithHttpMessagesAsync
{
    public static void MapListNamespacedCustomObjectWithHttpMessagesAsync<T>(this IEndpointRouteBuilder builder, Watcher<T>.WatchEvent? watchEvent = null)
        where T : CustomResource, new()
    {
        builder.MapGet("/apis/{group}/{version}/namespaces/{namespace}/{plural}",  async context =>
        {
            if(watchEvent is null)
            {
                var j = KubernetesJson.Serialize(new T());
                await context.Response.WriteAsync(j);
                return;
            }

            var json = KubernetesJson.Serialize(watchEvent);
            await context.Response.WriteAsync(json);
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(true);
            
        });
    }
}
