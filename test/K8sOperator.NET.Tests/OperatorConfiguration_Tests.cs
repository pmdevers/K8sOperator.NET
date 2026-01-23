using K8sOperator.NET.Configuration;

namespace K8sOperator.NET.Tests;

public class OperatorConfiguration_Tests
{
    [Test]
    public async Task ContainerTag_Should_DefaultToLatest_WhenNotSet()
    {
        // Arrange
        var config = new OperatorConfiguration
        {
            ContainerRegistry = "ghcr.io",
            ContainerRepository = "myorg/myapp"
        };

        // Act
        var tag = config.ContainerTag;

        // Assert
        await Assert.That(tag).IsEqualTo("latest");
    }

    [Test]
    public async Task ContainerTag_Should_ReturnLatest_WhenSetToEmptyString()
    {
        // Arrange
        var config = new OperatorConfiguration
        {
            ContainerRegistry = "ghcr.io",
            ContainerRepository = "myorg/myapp",
            ContainerTag = ""
        };

        // Act
        var tag = config.ContainerTag;

        // Assert
        await Assert.That(tag).IsEqualTo("latest");
    }

    [Test]
    public async Task ContainerTag_Should_ReturnLatest_WhenSetToWhitespace()
    {
        // Arrange
        var config = new OperatorConfiguration
        {
            ContainerRegistry = "ghcr.io",
            ContainerRepository = "myorg/myapp",
            ContainerTag = "   "
        };

        // Act
        var tag = config.ContainerTag;

        // Assert
        await Assert.That(tag).IsEqualTo("latest");
    }

    [Test]
    public async Task ContainerTag_Should_ReturnSetValue_WhenValid()
    {
        // Arrange
        var config = new OperatorConfiguration
        {
            ContainerRegistry = "ghcr.io",
            ContainerRepository = "myorg/myapp",
            ContainerTag = "1.0.0"
        };

        // Act
        var tag = config.ContainerTag;

        // Assert
        await Assert.That(tag).IsEqualTo("1.0.0");
    }

    [Test]
    public async Task ContainerImage_Should_BuildCorrectImage_WithAllProperties()
    {
        // Arrange
        var config = new OperatorConfiguration
        {
            ContainerRegistry = "ghcr.io",
            ContainerRepository = "myorg/myapp",
            ContainerTag = "1.0.0"
        };

        // Act
        var image = config.ContainerImage;

        // Assert
        await Assert.That(image).IsEqualTo("ghcr.io/myorg/myapp:1.0.0");
    }

    [Test]
    public async Task ContainerImage_Should_UseLatestTag_WhenTagNotSet()
    {
        // Arrange
        var config = new OperatorConfiguration
        {
            ContainerRegistry = "ghcr.io",
            ContainerRepository = "myorg/myapp"
        };

        // Act
        var image = config.ContainerImage;

        // Assert
        await Assert.That(image).IsEqualTo("ghcr.io/myorg/myapp:latest");
    }

    [Test]
    public async Task ContainerImage_Should_ThrowException_WhenRegistryIsEmpty()
    {
        // Arrange
        var config = new OperatorConfiguration
        {
            ContainerRegistry = "",
            ContainerRepository = "myorg/myapp"
        };

        // Act & Assert
        var exception = await Assert.That(() => config.ContainerImage)
            .Throws<InvalidOperationException>();

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.Message).Contains("ContainerRegistry must be configured");
    }

    [Test]
    public async Task ContainerImage_Should_ThrowException_WhenRegistryIsWhitespace()
    {
        // Arrange
        var config = new OperatorConfiguration
        {
            ContainerRegistry = "   ",
            ContainerRepository = "myorg/myapp"
        };

        // Act & Assert
        var exception = await Assert.That(() => config.ContainerImage)
            .Throws<InvalidOperationException>();

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.Message).Contains("ContainerRegistry must be configured");
    }

    [Test]
    public async Task ContainerImage_Should_ThrowException_WhenRepositoryIsEmpty()
    {
        // Arrange
        var config = new OperatorConfiguration
        {
            ContainerRegistry = "ghcr.io",
            ContainerRepository = ""
        };

        // Act & Assert
        var exception = await Assert.That(() => config.ContainerImage)
            .Throws<InvalidOperationException>();

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.Message).Contains("ContainerRepository must be configured");
    }

    [Test]
    public async Task ContainerImage_Should_ThrowException_WhenRepositoryIsWhitespace()
    {
        // Arrange
        var config = new OperatorConfiguration
        {
            ContainerRegistry = "ghcr.io",
            ContainerRepository = "   "
        };

        // Act & Assert
        var exception = await Assert.That(() => config.ContainerImage)
            .Throws<InvalidOperationException>();

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.Message).Contains("ContainerRepository must be configured");
    }

    [Test]
    public async Task ContainerImage_Should_TrimRegistryAndRepository()
    {
        // Arrange
        var config = new OperatorConfiguration
        {
            ContainerRegistry = "  ghcr.io  ",
            ContainerRepository = "  myorg/myapp  ",
            ContainerTag = "1.0.0"
        };

        // Act
        var image = config.ContainerImage;

        // Assert
        await Assert.That(image).IsEqualTo("ghcr.io/myorg/myapp:1.0.0");
    }

    [Test]
    public async Task Validate_Should_ThrowException_WhenOperatorNameIsEmpty()
    {
        // Arrange
        var config = new OperatorConfiguration
        {
            OperatorName = "",
            ContainerRegistry = "ghcr.io",
            ContainerRepository = "myorg/myapp"
        };

        // Act & Assert
        var exception = await Assert.That(() => config.Validate())
            .Throws<InvalidOperationException>();

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.Message).Contains("OperatorName must be configured");
    }

    [Test]
    public async Task Validate_Should_ThrowException_WhenNamespaceIsEmpty()
    {
        // Arrange
        var config = new OperatorConfiguration
        {
            OperatorName = "my-operator",
            Namespace = "",
            ContainerRegistry = "ghcr.io",
            ContainerRepository = "myorg/myapp"
        };

        // Act & Assert
        var exception = await Assert.That(() => config.Validate())
            .Throws<InvalidOperationException>();

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.Message).Contains("Namespace must be configured");
    }

    [Test]
    public async Task Validate_Should_ThrowException_WhenContainerRegistryIsEmpty()
    {
        // Arrange
        var config = new OperatorConfiguration
        {
            OperatorName = "my-operator",
            Namespace = "default",
            ContainerRegistry = "",
            ContainerRepository = "myorg/myapp"
        };

        // Act & Assert
        var exception = await Assert.That(() => config.Validate())
            .Throws<InvalidOperationException>();

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.Message).Contains("ContainerRegistry must be configured");
    }

    [Test]
    public async Task Validate_Should_ThrowException_WhenContainerRepositoryIsEmpty()
    {
        // Arrange
        var config = new OperatorConfiguration
        {
            OperatorName = "my-operator",
            Namespace = "default",
            ContainerRegistry = "ghcr.io",
            ContainerRepository = ""
        };

        // Act & Assert
        var exception = await Assert.That(() => config.Validate())
            .Throws<InvalidOperationException>();

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.Message).Contains("ContainerRepository must be configured");
    }

    [Test]
    public void Validate_Should_NotThrow_WhenAllPropertiesAreValid()
    {
        // Arrange
        var config = new OperatorConfiguration
        {
            OperatorName = "my-operator",
            Namespace = "default",
            ContainerRegistry = "ghcr.io",
            ContainerRepository = "myorg/myapp",
            ContainerTag = "1.0.0"
        };

        // Act & Assert - Should not throw
        config.Validate();
    }

    [Test]
    public async Task Validate_Should_NotThrow_WhenTagIsEmpty_UsesLatestDefault()
    {
        // Arrange
        var config = new OperatorConfiguration
        {
            OperatorName = "my-operator",
            Namespace = "default",
            ContainerRegistry = "ghcr.io",
            ContainerRepository = "myorg/myapp"
            // ContainerTag not set - should default to "latest"
        };

        // Act & Assert - Should not throw
        config.Validate();

        // Verify the image is valid
        await Assert.That(config.ContainerImage).IsEqualTo("ghcr.io/myorg/myapp:latest");
    }
}
