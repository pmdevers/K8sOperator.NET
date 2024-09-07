using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Text.Json;

namespace K8sOperator.NET.Tests.Mocks.Endpoints;
internal static class ReplaceNamespacedCustomObjectAsync
{
    public static void MapReplaceNamespacedCustomObjectAsync(this IEndpointRouteBuilder builder)
    {
        builder.MapPut("/apis/{group}/{version}/namespaces/{namespace}/{plural}/{name}", async context =>
         {
             // Mock replacing a custom resource
             var requestBody = await JsonSerializer.DeserializeAsync<TestResource>(context.Request.Body);

             var jsonResponse = JsonSerializer.Serialize(requestBody);
             context.Response.ContentType = "application/json";
             await context.Response.WriteAsync(jsonResponse);
         });
    }
}
