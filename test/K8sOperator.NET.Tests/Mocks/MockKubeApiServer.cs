using K8sOperator.NET.Tests.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using Microsoft.AspNetCore.Builder;
using k8s;
using k8s.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace K8sOperator.NET.Tests.Mocks;
public sealed class MockKubeApiServer : IDisposable
{
    private readonly IWebHost _server;

    public MockKubeApiServer(ITestOutputHelper testOutput, Action<IEndpointRouteBuilder>? builder = null)
    {
        _server = WebHost.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddRouting();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();

                if (testOutput != null)
                {
                    logging.AddTestOutput(testOutput);
                }
            })
            .UseKestrel(options => 
            { 
                options.Listen(IPAddress.Loopback, 0); 
            })
            .Configure(app =>
            {
                // Mock Kube API routes
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    builder?.Invoke(endpoints);

                    endpoints.Map("{*url}", (ILogger<MockKubeApiServer> logger, string url) =>
                    {
                        logger.LogInformation("route not handled: '{url}'", url);
                    });
                });
         }).Build();

         _server.Start();
    }

    public Uri Uri => _server.ServerFeatures.Get<IServerAddressesFeature>()!.Addresses
            .Select(a => new Uri(a)).First();

    // Method to get the mocked Kubernetes client
    public IKubernetes GetMockedKubernetesClient()
    {
        var config = new KubernetesClientConfiguration { Host = Uri.ToString() };
        return new Kubernetes(config);
    }

    public void Dispose()
    {
        _server.StopAsync();
        _server.WaitForShutdown();
        _server.Dispose();
    }
}
