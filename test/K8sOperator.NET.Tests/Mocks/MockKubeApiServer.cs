using k8s;
using K8sOperator.NET.Tests.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace K8sOperator.NET.Tests.Mocks;

public sealed class MockKubeApiServer : IDisposable
{
    private readonly IHost _server;

    public MockKubeApiServer(ITestOutputHelper testOutput, Action<IEndpointRouteBuilder>? endpoints = null)
    {
        var builder = WebApplication.CreateBuilder();

        builder.Services.AddRouting();
        builder.Logging.ClearProviders();
        if (testOutput != null)
        {
            builder.Logging.AddTestOutput(testOutput);
        }

        var app = builder.Build();
        // Mock Kube API routes
        app.UseRouting();

        endpoints?.Invoke(app);
        app.Map("{*url}", (ILogger<MockKubeApiServer> logger, string url) =>
        {
            logger.LogInformation("route not handled: '{url}'", url);
        });

        _server = builder.Build();
        _server.Start();
    }

    public Uri Uri => _server.GetTestServer().BaseAddress;

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
