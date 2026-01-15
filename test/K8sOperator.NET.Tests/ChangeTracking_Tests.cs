using k8s.Models;
using K8sOperator.NET.Tests.Fixtures;

namespace K8sOperator.NET.Tests;

public class ChangeTracker_Tests
{
    private readonly ChangeTracker _changeTracker = new ChangeTracker();

    [Test]
    public async Task IsResourceGenerationAlreadyHandled_Should_ReturnFalse_IfNotTrackedBefore()
    {
        // Arrange
        var resource = new TestResource
        {
            Metadata = new V1ObjectMeta
            {
                Uid = "resource-uid",
                Generation = 1
            }
        };

        // Act
        var result = _changeTracker.IsResourceGenerationAlreadyHandled(resource);

        // Assert
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task IsResourceGenerationAlreadyHandled_Should_ReturnTrue_IfAlreadyTracked()
    {
        // Arrange
        var resource = new TestResource
        {
            Metadata = new V1ObjectMeta
            {
                Uid = "resource-uid",
                Generation = 1
            }
        };

        // First, track the resource as handled
        _changeTracker.TrackResourceGenerationAsHandled(resource);

        // Act
        var result = _changeTracker.IsResourceGenerationAlreadyHandled(resource);

        // Assert
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task TrackResourceGenerationAsHandled_Should_UpdateTrackingCorrectly()
    {
        // Arrange
        var resource = new TestResource
        {
            Metadata = new V1ObjectMeta
            {
                Uid = "resource-uid",
                Generation = 2
            }
        };

        // Act
        _changeTracker.TrackResourceGenerationAsHandled(resource);

        // Check if it's tracked
        var result = _changeTracker.IsResourceGenerationAlreadyHandled(resource);

        // Assert
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task TrackResourceGenerationAsDeleted_Should_RemoveTrackingForDeletedResource()
    {
        // Arrange
        var resource = new TestResource
        {
            Metadata = new V1ObjectMeta
            {
                Uid = "resource-uid",
                Generation = 1
            }
        };

        // First, track the resource as handled
        _changeTracker.TrackResourceGenerationAsHandled(resource);

        // Act
        _changeTracker.TrackResourceGenerationAsDeleted(resource);

        // Check if it's still tracked
        var result = _changeTracker.IsResourceGenerationAlreadyHandled(resource);

        // Assert
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task TrackResourceGenerationAsHandled_Should_NotThrow_WhenGenerationIsNull()
    {
        // Arrange
        var resource = new TestResource
        {
            Metadata = new V1ObjectMeta
            {
                Uid = "resource-uid",
                Generation = null
            }
        };

        // Act
        _changeTracker.TrackResourceGenerationAsHandled(resource);

        // Assert
        var result = _changeTracker.IsResourceGenerationAlreadyHandled(resource);

        await Assert.That(result).IsFalse();

    }
}
