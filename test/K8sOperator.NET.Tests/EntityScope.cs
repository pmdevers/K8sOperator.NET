using k8s;
using K8sOperator.NET.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace K8sOperator.NET.Tests;
public class EntityScope_Tests
{
    [Fact]
    public void NamespacedScope_should_use_NamespacedClient()
    {
        var builder = OperatorHost.CreateOperatorApplicationBuilder();

        builder.AddController<TestController>()
            .ForNamespace("test");

        builder.Services.RemoveAll<IKubernetes>();
        builder.Services.AddSingleton(x => Substitute.For<IKubernetes>());

        var app = builder.Build();

        var watcher = app.DataSource.GetWatchers(app.ServiceProvider).ToList();

        watcher.Should().HaveCount(1);
    }

    [Fact]
    public void ClusterScope_should_use_NamespacedClient()
    {
        var builder = OperatorHost.CreateOperatorApplicationBuilder();

        builder.Services.RemoveAll<IKubernetes>();
        builder.Services.AddSingleton(x => Substitute.For<IKubernetes>());


        builder.AddController<Test2Controller>();
        
        var app = builder.Build();

        var watcher = app.DataSource.GetWatchers(app.ServiceProvider).ToList();

        watcher.Should().HaveCount(1);
    }

    private class TestController : Controller<TestResource> { }

    private class Test2Controller : Controller<ClusterResource> { }

    public class ClusterResource : TestResource
    {

    }
}
