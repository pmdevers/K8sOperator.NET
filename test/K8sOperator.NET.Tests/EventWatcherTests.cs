using Microsoft.Extensions.Logging;
using k8s.Autorest;
using k8s.Models;
using k8s;
using K8sOperator.NET.Extensions;
using K8sOperator.NET.Metadata;
using K8sOperator.NET.Models;

namespace K8sOperator.NET.Tests;

public class EventWatcherTests
{
    private readonly EventWatcher<TestResource> _sut;
    private readonly IKubernetes _kubernetesClient;
    private readonly Controller<TestResource> _controller;
    private readonly List<object> _metadata;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _logger;

    public EventWatcherTests()
    {
        _kubernetesClient = Substitute.For<IKubernetes>();
        _controller = Substitute.For<Controller<TestResource>>();
        _metadata =
        [
            new KubernetesEntityAttribute { Group = "group", ApiVersion = "v1", PluralName = "test-resources" },
            new WatchNamespaceMetadata("default"),
            new LabelSelectorMetadata("app=test")
        ];

        _loggerFactory = Substitute.For<ILoggerFactory>();
        _logger = Substitute.For<ILogger>();
        _loggerFactory.CreateLogger(Arg.Any<string>()).Returns(_logger);

        // Initialize the SUT (System Under Test)
        _sut = new EventWatcher<TestResource>(_kubernetesClient, _controller, _metadata, _loggerFactory);
    }

    [Fact]
    public async Task Start_ShouldStartWatcherAndProcessEvents()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var fakeWatchResponse = new FakeWatchResponse<TestResource>
        {
            WatchEvents =
            [
                (WatchEventType.Added, new TestResource { Metadata = new V1ObjectMeta { Name = "test-resource-1" } }),
                (WatchEventType.Modified, new TestResource { Metadata = new V1ObjectMeta { Name = "test-resource-2" } }),
            ]
        };

        _kubernetesClient.CustomObjects.ListNamespacedCustomObjectWithHttpMessagesAsync(
            Arg.Any<string>(),
            Arg.Any<string>(), 
            Arg.Any<string>(), 
            Arg.Any<string>(),
            watch: Arg.Any<bool>(), 
            allowWatchBookmarks: Arg.Any<bool>(),
            labelSelector: Arg.Any<string>(),
            timeoutSeconds: Arg.Any<int>(),
            cancellationToken: Arg.Any<CancellationToken>()
        ).Returns(fakeWatchResponse);

        // Act
        await _sut.Start(cts.Token);

        // Assert
        _logger.Received(1).BeginWatch("default", "test-resources", "app=test");

        await _controller.Received(1).AddOrModifyAsync(Arg.Any<TestResource>(), Arg.Any<CancellationToken>());
        _logger.Received(1).EventReceived(WatchEventType.Added, Arg.Any<TestResource>());

        _logger.Received(1).EndWatch("default", "test-resources", "app=test");
    }

    [Fact]
    public async Task OnEvent_ShouldHandleAddedEvent()
    {
        // Arrange
        var resource = new TestResource { Metadata = new V1ObjectMeta { Name = "test-resource" } };

        // Act
        //_sut.InvokeOnEvent(WatchEventType.Added, resource);

        // Assert
        await _controller.Received(1).AddOrModifyAsync(resource, Arg.Any<CancellationToken>());
        _logger.Received(1).EventReceived(WatchEventType.Added, resource);
    }

    [Fact]
    public async Task OnEvent_ShouldHandleDeletedEvent()
    {
        // Arrange
        var resource = new TestResource { Metadata = new V1ObjectMeta { Name = "test-resource" } };

        // Act
        //_sut.InvokeOnEvent(WatchEventType.Deleted, resource);

        // Assert
        await _controller.Received(1).DeleteAsync(resource, Arg.Any<CancellationToken>());
        _logger.Received(1).EventReceived(WatchEventType.Deleted, resource);
    }

    [Fact]
    public void OnError_ShouldLogError()
    {
        // Arrange
        var exception = new Exception("Test error");

        // Act
        //_sut.InvokeOnError(exception);

        // Assert
        _logger.Received(1).LogError(exception, "Watcher error");
    }

    [Fact]
    public async Task HandleFinalizeAsync_ShouldCallFinalizeAndRemoveFinalizer()
    {
        // Arrange
        var resource = new TestResource
        {
            Metadata = new V1ObjectMeta
            {
                Name = "test-resource",
                Finalizers = ["test-finalizer"]
            }
        };

        // Act
        //await _sut.InvokeHandleFinalizeAsync(resource);

        // Assert
        await _controller.Received(1).FinalizeAsync(resource, Arg.Any<CancellationToken>());
        await _kubernetesClient.CustomObjects.Received(1).ReplaceNamespacedCustomObjectAsync<TestResource>(
            Arg.Any<TestResource>(),
            Arg.Any<string>(), 
            Arg.Any<string>(),
            Arg.Any<string>(), 
            Arg.Any<string>(),
            Arg.Any<string>(),
            cancellationToken: Arg.Any<CancellationToken>()
        );
        _logger.Received(1).RemoveFinalizer(resource);
    }

    [Fact]
    public async Task HandleErrorEventAsync_ShouldLogErrorAndCallErrorAsync()
    {
        // Arrange
        var resource = new TestResource { Metadata = new V1ObjectMeta { Name = "test-resource" } };

        // Act
        //await _sut.InvokeHandleErrorEventAsync(resource);

        // Assert
        await _controller.Received(1).ErrorAsync(resource, Arg.Any<CancellationToken>());
        _logger.Received(1).HandleError(resource);
    }
}

// Fake Kubernetes Watch Response Helper
public class FakeWatchResponse<T> : HttpOperationResponse<object>
{
    public List<(WatchEventType, T)> WatchEvents { get; set; } = [];

    public IAsyncEnumerable<(WatchEventType, T)> WatchAsync(Action<Exception> onError, CancellationToken cancellationToken)
    {
        return WatchEvents.ToAsyncEnumerable();
    }
}

// TestResource for the sake of this test
public class TestResource : CustomResource
{
}


