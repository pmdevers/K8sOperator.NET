using k8s;
using k8s.Models;
using K8sOperator.NET.Tests.Logging;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using System.Net;
using System.Reflection;
using System.Text.Json;

namespace K8sOperator.NET.Tests.Mocks;

public class MockKubeApiBuilder(IEndpointRouteBuilder builder)
{
    private readonly IEndpointRouteBuilder _builder = builder;

    public CustomObjectsImpl CustomObjects => new(_builder);

    public class CustomObjectsImpl(IEndpointRouteBuilder builder)
    {
        public void WatchListClusterCustomObjectAsync<T>(Watcher<T>.WatchEvent? watchEvent = null, string ns = "default")
             where T : CustomResource, new()
        {
            var attr = typeof(T).GetCustomAttribute<KubernetesEntityAttribute>();
            var group = attr?.Group ?? throw new InvalidOperationException($"KubernetesEntityAttribute.Group not defined on {typeof(T).FullName}");
            var version = attr?.ApiVersion ?? throw new InvalidOperationException($"KubernetesEntityAttribute.Version not defined on {typeof(T).FullName}");
            var plural = attr?.PluralName ?? throw new InvalidOperationException($"KubernetesEntityAttribute.Plural not defined on {typeof(T).FullName}");

            builder.MapGet($"/apis/{group}/{version}/namespaces/{ns}/{plural}", async context =>
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

        public void ReplaceNamespacedCustomObjectAsync<T>(string ns = "default", Action<T?>? resource = null)
            where T : CustomResource, new()
        {
            var attr = typeof(T).GetCustomAttribute<KubernetesEntityAttribute>();
            var group = attr?.Group ?? throw new InvalidOperationException($"KubernetesEntityAttribute.Group not defined on {typeof(T).FullName}");
            var version = attr?.ApiVersion ?? throw new InvalidOperationException($"KubernetesEntityAttribute.Version not defined on {typeof(T).FullName}");
            var plural = attr?.PluralName ?? throw new InvalidOperationException($"KubernetesEntityAttribute.Plural not defined on {typeof(T).FullName}");

            builder.MapPut($"/apis/{group}/{version}/namespaces/{ns}/{plural}/{{name}}", async context =>
            {
                // Mock replacing a custom resource
                var requestBody = await JsonSerializer.DeserializeAsync<T>(context.Request.Body);

                resource?.Invoke(requestBody);

                var jsonResponse = JsonSerializer.Serialize(requestBody);
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(jsonResponse);
            });
        }
    }
}

public sealed class MockKubeApiServer : IDisposable
{
    private readonly IHost _server;

    public MockKubeApiServer(Action<MockKubeApiBuilder>? configureApi = null)
    {
        _server = new HostBuilder()
            .ConfigureWebHost(config =>
            {
                config.ConfigureServices(services =>
                {
                    services.AddRouting();
                });
                config.UseKestrel(options => { options.Listen(IPAddress.Loopback, 0); });
                config.Configure(app =>
                {
                    // Mock Kube API routes
                    app.UseRouting();

                    app.UseEndpoints(endpoints =>
                    {
                        var builder = new MockKubeApiBuilder(endpoints);
                        configureApi?.Invoke(builder);
                        endpoints.Map("{*url}", (ILogger<MockKubeApiServer> logger, string url) =>
                        {
                            var safeUrl = url.Replace("\r", string.Empty).Replace("\n", string.Empty);

                            if (logger.IsEnabled(LogLevel.Information))
                                logger.LogInformation("route not handled: '{url}'", safeUrl);
                        });
                    });
                });
                config.ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddTestLogging(TestContext.Current);
                });
            })
            .Build();

        _server.Start();

        Client = new Kubernetes(GetKubernetesClientConfiguration());
    }

    public Uri Uri => _server.Services.GetRequiredService<IServer>()
            .Features
            .Get<IServerAddressesFeature>()!.Addresses
            .Select(a => new Uri(a)).First();

    public KubernetesClientConfiguration GetKubernetesClientConfiguration()
        => new() { Host = Uri.ToString() };

    public IKubernetes Client { get; }

    public void Dispose()
    {
        try
        {
            _server?.StopAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
        }
        catch { /* Ignore disposal errors */ }
        finally
        {
            _server?.Dispose();
        }
    }
}
