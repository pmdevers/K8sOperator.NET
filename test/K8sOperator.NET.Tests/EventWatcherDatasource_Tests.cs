using k8s.Models;
using K8sOperator.NET.Builder;
using K8sOperator.NET.Metadata;
using K8sOperator.NET.Tests.Fixtures;
using K8sOperator.NET.Tests.Mocks;

namespace K8sOperator.NET.Tests;

public class EventWatcherDatasource_Tests
{
    private static ServiceProvider CreateServiceProvider()
    {
        var server = new MockKubeApiServer();
        var services = new ServiceCollection();
        services.AddSingleton(sp => server.Client);
        services.AddSingleton(sp => LoggerFactory.Create(builder => { }));
        return services.BuildServiceProvider();
    }

    private static List<object> CreateMetadata()
    {
        return
        [
            OperatorNameAttribute.Default,
            DockerImageAttribute.Default,
            NamespaceAttribute.Default
        ];
    }

    [Test]
    public async Task Constructor_Should_InitializeWithServiceProviderAndMetadata()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var metadata = CreateMetadata();

        // Act
        var datasource = new EventWatcherDatasource(serviceProvider, metadata);

        // Assert
        await Assert.That(datasource).IsNotNull();
        await Assert.That(datasource.ServiceProvider).IsEqualTo(serviceProvider);
        await Assert.That(datasource.Metadata).IsEqualTo(metadata);
    }

    [Test]
    public async Task Add_Should_AddControllerToDataSource()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var metadata = CreateMetadata();
        var datasource = new EventWatcherDatasource(serviceProvider, metadata);

        // Act
        var builder = datasource.Add<TestController>();

        // Assert
        await Assert.That(builder).IsNotNull();
    }

    [Test]
    public async Task Add_Should_ReturnConventionBuilder()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var metadata = CreateMetadata();
        var datasource = new EventWatcherDatasource(serviceProvider, metadata);

        // Act
        var result = datasource.Add<TestController>();

        // Assert
        await Assert.That(result).IsTypeOf<ConventionBuilder<ControllerBuilder>>();
    }

    [Test]
    public async Task GetWatchers_Should_ReturnEmptyWhenNoControllersAdded()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var metadata = CreateMetadata();
        var datasource = new EventWatcherDatasource(serviceProvider, metadata);

        // Act
        var watchers = datasource.GetWatchers().ToList();

        // Assert
        await Assert.That(watchers).IsEmpty();
    }

    [Test]
    public async Task GetWatchers_Should_ReturnSingleWatcherWhenOneControllerAdded()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var metadata = CreateMetadata();
        var datasource = new EventWatcherDatasource(serviceProvider, metadata);

        datasource.Add<TestController>();

        // Act
        var watchers = datasource.GetWatchers().ToList();

        // Assert
        await Assert.That(watchers).Count().IsEqualTo(1);
        await Assert.That(watchers[0]).IsNotNull();
        await Assert.That(watchers[0].Controller).IsTypeOf<TestController>();
    }

    [Test]
    public async Task GetWatchers_Should_ReturnMultipleWatchersWhenMultipleControllersAdded()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var metadata = CreateMetadata();
        var datasource = new EventWatcherDatasource(serviceProvider, metadata);

        datasource.Add<TestController>();
        datasource.Add<AnotherTestController>();

        // Act
        var watchers = datasource.GetWatchers().ToList();

        // Assert
        await Assert.That(watchers).Count().IsEqualTo(2);
        await Assert.That(watchers[0].Controller).IsTypeOf<TestController>();
        await Assert.That(watchers[1].Controller).IsTypeOf<AnotherTestController>();
    }

    [Test]
    public async Task GetWatchers_Should_ApplyConventionsToControllerBuilder()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var metadata = CreateMetadata();
        var datasource = new EventWatcherDatasource(serviceProvider, metadata);
        var conventionApplied = false;

        datasource.Add<TestController>()
            .Add(builder =>
            {
                conventionApplied = true;
            });

        // Act
        var watchers = datasource.GetWatchers().ToList();

        // Assert
        await Assert.That(conventionApplied).IsTrue();
        await Assert.That(watchers).Count().IsEqualTo(1);
    }

    [Test]
    public async Task GetWatchers_Should_ApplyMultipleConventionsInOrder()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var metadata = CreateMetadata();
        var datasource = new EventWatcherDatasource(serviceProvider, metadata);
        var conventionOrder = new List<int>();

        datasource.Add<TestController>()
            .Add(builder => conventionOrder.Add(1))
            .Add(builder => conventionOrder.Add(2))
            .Add(builder => conventionOrder.Add(3));

        // Act
        var watchers = datasource.GetWatchers().ToList();

        // Assert
        await Assert.That(conventionOrder).IsEquivalentTo([1, 2, 3]);
    }

    [Test]
    public async Task GetWatchers_Should_CreateEventWatcherWithMetadata()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var metadata = CreateMetadata();
        var datasource = new EventWatcherDatasource(serviceProvider, metadata);

        datasource.Add<TestController>();

        // Act
        var watchers = datasource.GetWatchers().ToList();

        // Assert
        await Assert.That(watchers[0].Metadata).IsNotNull();
        await Assert.That(watchers[0].Metadata).Count().IsGreaterThan(0);
    }

    [Test]
    public async Task GetWatchers_Should_IncludeGlobalMetadataInWatcher()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var metadata = CreateMetadata();
        var datasource = new EventWatcherDatasource(serviceProvider, metadata);

        datasource.Add<TestController>();

        // Act
        var watchers = datasource.GetWatchers().ToList();

        // Assert
        var watcherMetadata = watchers[0].Metadata;
        await Assert.That(watcherMetadata.OfType<OperatorNameAttribute>()).HasSingleItem();
        await Assert.That(watcherMetadata.OfType<DockerImageAttribute>()).HasSingleItem();
        await Assert.That(watcherMetadata.OfType<NamespaceAttribute>()).HasSingleItem();
    }

    [Test]
    public async Task GetWatchers_Should_IncludeResourceMetadataInWatcher()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var metadata = CreateMetadata();
        var datasource = new EventWatcherDatasource(serviceProvider, metadata);

        datasource.Add<TestController>();

        // Act
        var watchers = datasource.GetWatchers().ToList();

        // Assert
        var watcherMetadata = watchers[0].Metadata;
        var kubernetesEntity = watcherMetadata.OfType<KubernetesEntityAttribute>().FirstOrDefault();
        await Assert.That(kubernetesEntity).IsNotNull();
        await Assert.That(kubernetesEntity!.Group).IsEqualTo("unittest");
        await Assert.That(kubernetesEntity.ApiVersion).IsEqualTo("v1");
        await Assert.That(kubernetesEntity.Kind).IsEqualTo("TestResource");
    }

    [Test]
    public async Task GetWatchers_Should_CreateNewWatcherInstancesEachTime()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var metadata = CreateMetadata();
        var datasource = new EventWatcherDatasource(serviceProvider, metadata);

        datasource.Add<TestController>();

        // Act
        var watchers1 = datasource.GetWatchers().ToList();
        var watchers2 = datasource.GetWatchers().ToList();

        // Assert
        await Assert.That(watchers1[0]).IsNotEqualTo(watchers2[0]);
    }

    [Test]
    public async Task GetWatchers_Should_CreateWatcherForCorrectResourceType()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var metadata = CreateMetadata();
        var datasource = new EventWatcherDatasource(serviceProvider, metadata);

        datasource.Add<TestController>();

        // Act
        var watchers = datasource.GetWatchers().ToList();

        // Assert
        await Assert.That(watchers[0].Controller.ResourceType).IsEqualTo(typeof(TestResource));
    }

    [Test]
    public async Task GetWatchers_Should_HandleMultipleControllersWithDifferentResourceTypes()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var metadata = CreateMetadata();
        var datasource = new EventWatcherDatasource(serviceProvider, metadata);

        datasource.Add<TestController>();
        datasource.Add<AnotherTestController>();

        // Act
        var watchers = datasource.GetWatchers().ToList();

        // Assert
        await Assert.That(watchers).Count().IsEqualTo(2);
        await Assert.That(watchers[0].Controller.ResourceType).IsEqualTo(typeof(TestResource));
        await Assert.That(watchers[1].Controller.ResourceType).IsEqualTo(typeof(AnotherTestResource));
    }

    [Test]
    public async Task Metadata_Should_BeAccessibleProperty()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var metadata = CreateMetadata();
        var datasource = new EventWatcherDatasource(serviceProvider, metadata);

        // Act & Assert
        await Assert.That(datasource.Metadata).IsEqualTo(metadata);
        await Assert.That(datasource.Metadata).Count().IsEqualTo(3);
    }

    [Test]
    public async Task ServiceProvider_Should_BeAccessibleProperty()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var metadata = CreateMetadata();
        var datasource = new EventWatcherDatasource(serviceProvider, metadata);

        // Act & Assert
        await Assert.That(datasource.ServiceProvider).IsEqualTo(serviceProvider);
    }

    [Test]
    public async Task GetWatchers_Should_YieldWatchersLazily()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var metadata = CreateMetadata();
        var datasource = new EventWatcherDatasource(serviceProvider, metadata);
        var buildCount = 0;

        datasource.Add<TestController>()
            .Add(_ => buildCount++);

        // Act
        var watcherEnumerable = datasource.GetWatchers();

        // Assert - Not built yet
        await Assert.That(buildCount).IsEqualTo(0);

        // Enumerate to trigger build
        var watchers = watcherEnumerable.ToList();
        await Assert.That(buildCount).IsEqualTo(1);
    }

    // Test helper classes
    [LabelSelector("app=test")]
    [Finalizer("test-finalizer")]
    private class TestController : OperatorController<TestResource>
    {
    }

    [LabelSelector("app=another")]
    private class AnotherTestController : OperatorController<AnotherTestResource>
    {
    }

    [KubernetesEntity(Group = "unittest", ApiVersion = "v1", Kind = "AnotherTestResource", PluralName = "anothertestresources")]
    private class AnotherTestResource : CustomResource
    {
    }
}
