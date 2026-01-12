using k8s;
using K8sOperator.NET.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;


namespace K8sOperator.NET.Tests.Mocks.Endpoints;

internal static class WatchNamespaced
{
    public static void MapWatchNamespacedCustomObjectAsync<T>(this IEndpointRouteBuilder builder, Watcher<T>.WatchEvent? watchEvent = null)
        where T : CustomResource, new()
    {
        // The correct URL pattern for Kubernetes watch API is the same as list but with ?watch=true query parameter
        builder.MapGet("/apis/{group}/{version}/namespaces/{namespace}/{plural}", async context =>
        {
            var isWatch = context.Request.Query["watch"].ToString() == "true";
            
            if (!isWatch || watchEvent is null)
            {
                var j = KubernetesJson.Serialize(new T());
                await context.Response.WriteAsync(j);
                return;
            }
            
            // For watch requests, send the event as newline-delimited JSON
            var json = KubernetesJson.Serialize(watchEvent);
            await context.Response.WriteAsync(json);
            await context.Response.WriteAsync("\n");
            await context.Response.Body.FlushAsync();
            await Task.Delay(TimeSpan.FromMilliseconds(100)).ConfigureAwait(true);
        });
    }
}
