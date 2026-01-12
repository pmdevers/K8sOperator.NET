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
    private readonly WebApplication _server;
    private readonly TestServer _testServer;

    public MockKubeApiServer(ITestOutputHelper testOutput, Action<IEndpointRouteBuilder>? endpoints = null)
    {
        var builder = WebApplication.CreateBuilder();

        builder.Services.AddRouting();

        builder.Logging.ClearProviders();
        if (testOutput != null)
        {
            builder.Logging.AddTestOutput(testOutput);
        }

        builder.WebHost.UseTestServer();

        _server = builder.Build();

        _testServer = _server.GetTestServer();

        // Mock Kube API routes
        _server.UseRouting();

        endpoints?.Invoke(_server);
        _server.Map("{*url}", (ILogger<MockKubeApiServer> logger, string url) =>
        {
            var safeUrl = url.Replace("\r", string.Empty).Replace("\n", string.Empty);
            logger.LogInformation("route not handled: '{url}'", safeUrl);
        });

        _server.Start();
    }

    public Uri Uri => _testServer.BaseAddress;

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
        _testServer.Dispose();
    }
}
