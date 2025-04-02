namespace K8sOperator.NET.Tests;

public class ControllerTests
{
    // Base functionality tests
    [Fact]
    public async Task AddOrModifyAsync_Should_Return_CompletedTask()
    {
        // Arrange
        var controller = new MyController();

        // Act
        await controller.AddOrModifyAsync(new TestResource(), CancellationToken.None);
        
        Assert.True(true);
    }

    [Fact]
    public async Task DeleteAsync_Should_Return_CompletedTask()
    {
        // Arrange
        var controller = new MyController();

        // Act
        await controller.DeleteAsync(new TestResource(), CancellationToken.None);

        // Assert
        Assert.True(true);
    }

    [Fact]
    public async Task FinalizeAsync_Should_Return_CompletedTask()
    {
        // Arrange
        var controller = new MyController();

        // Act
        await controller.FinalizeAsync(new TestResource(), CancellationToken.None);

        Assert.True(true);
    }

    [Fact]
    public async Task BookmarkAsync_Should_Return_CompletedTask()
    {
        // Arrange
        var controller = new MyController();

        // Act
        await controller.BookmarkAsync(new TestResource(), CancellationToken.None);

        // Assert
        Assert.True(true);
    }

    [Fact]
    public async Task ErrorAsync_Should_Return_CompletedTask()
    {
        // Arrange
        var controller = new MyController();

        // Act
        await controller.ErrorAsync(new TestResource(), CancellationToken.None);

        // Assert
        Assert.True(true);
    }

    // Test overriding in derived classes
    [Fact]
    public async Task Overridden_AddOrModifyAsync_Should_Call_CustomImplementation()
    {
        // Arrange
        var derivedController = new DerivedTestController();
        var resource = new TestResource();

        // Act
        await derivedController.AddOrModifyAsync(resource, CancellationToken.None);

        
        resource.Status.Should().NotBeNull();
        resource.Status?.Status.Should().Be("Changed");
    }

    // You can also extend these tests for DeleteAsync, FinalizeAsync, BookmarkAsync, and ErrorAsync
}


public class DerivedTestController : MyController
{
    public override Task AddOrModifyAsync(TestResource resource, CancellationToken cancellationToken)
    {
        resource.Status = new()
        {
            Status = "Changed"
        };

        return Task.CompletedTask;
    }
}
