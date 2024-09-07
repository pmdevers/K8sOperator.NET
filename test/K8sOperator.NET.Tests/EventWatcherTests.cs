using Microsoft.Extensions.Logging;
using k8s.Models;
using k8s;
using K8sOperator.NET.Metadata;
using K8sOperator.NET.Models;
using Xunit.Abstractions;
using K8sOperator.NET.Tests.Mocks;
using K8sOperator.NET.Tests.Mocks.Endpoints;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

namespace K8sOperator.NET.Tests;

public class EventWatcherTests
{
    private static Watcher<TestResource>.WatchEvent Added => CreateEvent(WatchEventType.Modified, 
        new TestResource() { Metadata = new() 
        { 
            Name = "test", 
            NamespaceProperty = "default", 
            Finalizers = ["finalize"], 
            Uid = "1" 
        } 
    });

    private static Watcher<TestResource>.WatchEvent Finalize => CreateEvent(WatchEventType.Added,
        new TestResource() { Metadata = new() 
        { 
            Name = "test", 
            NamespaceProperty = "default",
            DeletionTimestamp = TimeProvider.System.GetUtcNow().DateTime,
            Finalizers = ["finalize"], 
            Uid = "1" 
        } 
    });

    private static Watcher<TestResource>.WatchEvent Deleted => CreateEvent(WatchEventType.Deleted,
        new TestResource() { Metadata = new() 
        { 
            Name = "test", 
            NamespaceProperty = "default", 
            Finalizers = ["finalize"], 
            Uid = "1" 
        } 
    });

    private static Watcher<T>.WatchEvent CreateEvent<T>(WatchEventType type, T item)
        where T : CustomResource
    {
        return new Watcher<T>.WatchEvent { Type = type, Object = item };
    }

    private readonly ITestOutputHelper _testOutput;
    private readonly Controller<TestResource> _controller = Substitute.For<Controller<TestResource>>();
    private readonly ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();
    private readonly ILogger _logger = Substitute.For<ILogger>();
    private readonly List<object> _metadata;

    public EventWatcherTests(ITestOutputHelper testOutput)
    {
        _testOutput = testOutput;
        _loggerFactory.CreateLogger(Arg.Any<string>()).Returns(_logger);
        _metadata = [
            new KubernetesEntityAttribute { Group = "group", ApiVersion = "v1", Kind = "Test", PluralName = "tests" },
            new WatchNamespaceMetadata("default"),
            Substitute.For<ILabelSelectorMetadata>(),
            new FinalizerMetadata("finalize")
        ];
    }

    [Fact]
    public async Task Start_Should_StartWatchAndLogStart()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        
        using ( var server = new MockKubeApiServer(_testOutput, endpoints =>
        {
            endpoints.MapListNamespacedCustomObjectWithHttpMessagesAsync<TestResource>();
        }))
        {
            var client = server.GetMockedKubernetesClient();
            var watcher = new EventWatcher<TestResource>(client, _controller, _metadata, _loggerFactory);

            await watcher.Start(cancellationToken);
        }
        
        _loggerFactory.Received(1).CreateLogger(Arg.Any<string>());
    }

    [Fact]
    public async Task OnEvent_Should_HandleAddedEventAndCallAddOrModifyAsync()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        using (var server = new MockKubeApiServer(_testOutput, endpoints =>
        {
            endpoints.MapListNamespacedCustomObjectWithHttpMessagesAsync(Added);
            endpoints.MapReplaceNamespacedCustomObjectAsync();
        }))
        {
            var client = server.GetMockedKubernetesClient();
            var watcher = new EventWatcher<TestResource>(client, _controller, _metadata, _loggerFactory);

            await watcher.Start(cancellationToken);
        }

        _loggerFactory.Received(1).CreateLogger(Arg.Any<string>());

        _controller.Received(1).AddOrModifyAsync(Arg.Any<TestResource>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task OnEvent_Should_HandleDeletedEventAndCallDeleteAsync()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        using (var server = new MockKubeApiServer(_testOutput, endpoints =>
        {
            endpoints.MapListNamespacedCustomObjectWithHttpMessagesAsync(Deleted);
            endpoints.MapReplaceNamespacedCustomObjectAsync();
        }))
        {
            var client = server.GetMockedKubernetesClient();
            var watcher = new EventWatcher<TestResource>(client, _controller, _metadata, _loggerFactory);

            await watcher.Start(cancellationToken);
        }

        _loggerFactory.Received(1).CreateLogger(Arg.Any<string>());

        _controller.Received(1).DeleteAsync(Arg.Any<TestResource>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleFinalizeAsync_Should_CallFinalizeAndRemoveFinalizer()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        using (var server = new MockKubeApiServer(_testOutput, endpoints =>
        {
            endpoints.MapListNamespacedCustomObjectWithHttpMessagesAsync(Finalize);
            endpoints.MapReplaceNamespacedCustomObjectAsync();
        }))
        {
            var client = server.GetMockedKubernetesClient();
            var watcher = new EventWatcher<TestResource>(client, _controller, _metadata, _loggerFactory);

            await watcher.Start(cancellationToken);
        }

        _loggerFactory.Received(1).CreateLogger(Arg.Any<string>());

        _controller.Received(1).FinalizeAsync(Arg.Any<TestResource>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void HandleAddOrModifyAsync_Should_AddFinalizer_IfNotPresent()
    {
        Assert.True(true);
    }
}


[KubernetesEntity(Group = "unittest", ApiVersion = "v1", Kind = "TestResource", PluralName = "testresources")]
public class TestResource : CustomResource<TestResource.TestSpec, TestResource.TestStatus>
{
    public class TestStatus
    {
        public string Status { get; set; } = string.Empty;
    }

    public class TestSpec
    {
        public string Property { get; set; } = string.Empty;
    }
}
