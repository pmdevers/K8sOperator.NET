using k8s;
using K8sOperator.NET.Tests.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;

namespace K8sOperator.NET.Tests.Mocks;

public sealed class MockKubeApiServer : IDisposable
{
    private readonly IHost _server;

    public MockKubeApiServer(TestContext testOutput, Action<IEndpointRouteBuilder>? builder = null)
    {
        _server = new HostBuilder()
            .ConfigureWebHost(config =>
            {
                config.ConfigureServices(services =>
                {
                    services.AddRouting();
                });
                config.UseKestrel(options => { options.Listen(IPAddress.Loopback, 8888); });
                config.Configure(app =>
                {
                    // Mock Kube API routes
                    app.UseRouting();

                    app.UseEndpoints(endpoints =>
                    {
                        builder?.Invoke(endpoints);
                        endpoints.Map("{*url}", (ILogger<MockKubeApiServer> logger, string url) =>
                        {
                            var safeUrl = url.Replace("\r", string.Empty).Replace("\n", string.Empty);
                            logger.LogInformation("route not handled: '{url}'", safeUrl);
                        });
                    });
                });
                config.ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    if (testOutput != null)
                    {
                        logging.AddTestLogging(testOutput);
                    }
                });
            })
            .Build();


        _server.Start();
    }

    public Uri Uri => _server.Services.GetRequiredService<IServer>()
            .Features
            .Get<IServerAddressesFeature>()!.Addresses
            .Select(a => new Uri(a)).First();

    // Method to get the mocked Kubernetes client
    public IKubernetes GetMockedKubernetesClient()
    {
        var config = new KubernetesClientConfiguration { Host = Uri.ToString() };
        return new Kubernetes(config);
    }

    public void Dispose()
    {
        _server.Dispose();
    }
}
