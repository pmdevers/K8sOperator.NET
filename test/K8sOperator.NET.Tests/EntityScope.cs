using k8s;
using K8sOperator.NET.Extensions;
using K8sOperator.NET.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K8sOperator.NET.Tests;
public class EntityScope_Tests
{
    [Fact]
    public void NamespacedScope_should_use_NamespacedClient()
    {
        var builder = OperatorHost.CreateOperatorApplicationBuilder();

        builder.AddController<TestController>()
            .WatchNamespace("test");

        builder.Services.RemoveAll<IKubernetes>();
        builder.Services.AddTransient(x => Substitute.For<IKubernetes>());

        var app = builder.Build();

        var watcher = app.DataSource.GetWatchers(app.ServiceProvider).ToList();

        watcher.Should().HaveCount(1);
    }

    [Fact]
    public void ClusterScope_should_use_NamespacedClient()
    {
        var builder = OperatorHost.CreateOperatorApplicationBuilder();

        builder.Services.RemoveAll<IKubernetes>();
        builder.Services.AddTransient(x => Substitute.For<IKubernetes>());


        builder.AddController<Test2Controller>();
        
        var app = builder.Build();

        var watcher = app.DataSource.GetWatchers(app.ServiceProvider).ToList();

        watcher.Should().HaveCount(1);
    }

    private class TestController : Controller<TestResource> { }

    private class Test2Controller : Controller<ClusterResource> { }

    [EntityScopeMetadata(EntityScope.Cluster)]
    public class ClusterResource : TestResource
    {

    }
}
