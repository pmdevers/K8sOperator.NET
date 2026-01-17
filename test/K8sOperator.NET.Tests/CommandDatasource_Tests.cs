using K8sOperator.NET.Builder;
using K8sOperator.NET.Commands;

namespace K8sOperator.NET.Tests;

public class CommandDatasource_Tests
{
    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton(sp => Host.CreateDefaultBuilder().Build());
        return services.BuildServiceProvider();
    }

    [Test]
    public async Task Constructor_Should_InitializeWithServiceProvider()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();

        // Act
        var datasource = new CommandDatasource(serviceProvider);

        // Assert
        await Assert.That(datasource).IsNotNull();
        await Assert.That(datasource.ServiceProvider).IsEqualTo(serviceProvider);
    }

    [Test]
    public async Task Add_Should_AddCommandToDataSource()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var datasource = new CommandDatasource(serviceProvider);

        // Act
        var builder = datasource.Add<TestCommand>();

        // Assert
        await Assert.That(builder).IsNotNull();
    }

    [Test]
    public async Task Add_Should_ReturnConventionBuilder()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var datasource = new CommandDatasource(serviceProvider);

        // Act
        var result = datasource.Add<TestCommand>();

        // Assert
        await Assert.That(result).IsTypeOf<ConventionBuilder<CommandBuilder>>();
    }

    [Test]
    public async Task GetCommands_Should_ReturnEmptyWhenNoCommandsAdded()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var datasource = new CommandDatasource(serviceProvider);
        var host = serviceProvider.GetRequiredService<IHost>();

        // Act
        var commands = datasource.GetCommands(host).ToList();

        // Assert
        await Assert.That(commands).IsEmpty();
    }

    [Test]
    public async Task GetCommands_Should_ReturnSingleCommandWhenOneAdded()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var datasource = new CommandDatasource(serviceProvider);
        var host = serviceProvider.GetRequiredService<IHost>();

        datasource.Add<TestCommand>();

        // Act
        var commands = datasource.GetCommands(host).ToList();

        // Assert
        await Assert.That(commands).Count().IsEqualTo(1);
        await Assert.That(commands[0].Command).IsTypeOf<TestCommand>();
    }

    [Test]
    public async Task GetCommands_Should_ReturnMultipleCommandsInOrderAdded()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var datasource = new CommandDatasource(serviceProvider);
        var host = serviceProvider.GetRequiredService<IHost>();

        datasource.Add<TestCommand>();
        datasource.Add<AnotherTestCommand>();

        // Act
        var commands = datasource.GetCommands(host).ToList();

        // Assert
        await Assert.That(commands).Count().IsEqualTo(2);
        await Assert.That(commands[0].Command).IsTypeOf<TestCommand>();
        await Assert.That(commands[1].Command).IsTypeOf<AnotherTestCommand>();
    }

    [Test]
    public async Task GetCommands_Should_IncludeMetadataFromCommandAttribute()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var datasource = new CommandDatasource(serviceProvider);
        var host = serviceProvider.GetRequiredService<IHost>();

        datasource.Add<OperatorCommand>();

        // Act
        var commands = datasource.GetCommands(host).ToList();

        // Assert
        await Assert.That(commands).Count().IsEqualTo(1);
        var metadata = commands[0].Metadata.OfType<OperatorArgumentAttribute>().First();
        await Assert.That(metadata.Argument).IsEqualTo("operator");
        await Assert.That(metadata.Description).IsEqualTo("Starts the operator.");
    }

    [Test]
    public async Task GetCommands_Should_ApplyConventionsToCommandBuilder()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var datasource = new CommandDatasource(serviceProvider);
        var host = serviceProvider.GetRequiredService<IHost>();
        var conventionApplied = false;

        datasource.Add<TestCommand>()
            .Add(builder =>
            {
                conventionApplied = true;
            });

        // Act
        var commands = datasource.GetCommands(host).ToList();

        // Assert
        await Assert.That(conventionApplied).IsTrue();
        await Assert.That(commands).Count().IsEqualTo(1);
    }

    [Test]
    public async Task GetCommands_Should_ApplyMultipleConventionsInOrder()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var datasource = new CommandDatasource(serviceProvider);
        var host = serviceProvider.GetRequiredService<IHost>();
        var conventionOrder = new List<int>();

        datasource.Add<TestCommand>()
            .Add(builder => conventionOrder.Add(1))
            .Add(builder => conventionOrder.Add(2))
            .Add(builder => conventionOrder.Add(3));

        // Act
        var commands = datasource.GetCommands(host).ToList();

        // Assert
        await Assert.That(conventionOrder).IsEquivalentTo([1, 2, 3]);
    }

    [Test]
    public async Task GetCommands_Should_CreateNewCommandInstancesEachTime()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var datasource = new CommandDatasource(serviceProvider);
        var host = serviceProvider.GetRequiredService<IHost>();

        datasource.Add<TestCommand>();

        // Act
        var commands1 = datasource.GetCommands(host).ToList();
        var commands2 = datasource.GetCommands(host).ToList();

        // Assert
        await Assert.That(commands1[0].Command).IsNotEqualTo(commands2[0].Command);
    }

    [Test]
    public async Task GetCommands_Should_BuildCommandWithHost()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var datasource = new CommandDatasource(serviceProvider);
        var host = serviceProvider.GetRequiredService<IHost>();

        datasource.Add<TestCommand>();

        // Act
        var commands = datasource.GetCommands(host).ToList();

        // Assert
        var testCommand = (TestCommand)commands[0].Command;
        await Assert.That(testCommand.Host).IsEqualTo(host);
    }

    [Test]
    public async Task GetCommands_Should_OrderCommandsByOrderProperty()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var datasource = new CommandDatasource(serviceProvider);
        var host = serviceProvider.GetRequiredService<IHost>();

        // Add commands (they'll be ordered by Order property internally)
        datasource.Add<OperatorCommand>(); // Order = -2
        datasource.Add<InstallCommand>(); // Order = 1 (default)
        datasource.Add<TestCommand>(); // Order = 1 (default)

        // Act
        var commands = datasource.GetCommands(host).ToList();

        // Assert
        await Assert.That(commands).Count().IsEqualTo(3);
        // OperatorCommand should be first due to Order = -2
        await Assert.That(commands[0].Command).IsTypeOf<OperatorCommand>();
    }

    [Test]
    public async Task CommandInfo_Should_ContainCommandAndMetadata()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var datasource = new CommandDatasource(serviceProvider);
        var host = serviceProvider.GetRequiredService<IHost>();

        datasource.Add<OperatorCommand>();

        // Act
        var commandInfo = datasource.GetCommands(host).First();

        // Assert
        await Assert.That(commandInfo.Command).IsNotNull();
        await Assert.That(commandInfo.Metadata).IsNotNull();
        await Assert.That(commandInfo.Metadata).Count().IsEqualTo(1);
    }

    // Test helper classes
    [OperatorArgument("test", Description = "Test command")]
    private class TestCommand(IHost host) : IOperatorCommand
    {
        public IHost Host { get; } = host;

        public Task RunAsync(string[] args)
        {
            return Task.CompletedTask;
        }
    }

    [OperatorArgument("another", Description = "Another test command")]
    private class AnotherTestCommand(IHost host) : IOperatorCommand
    {
        private readonly IHost _host = host;

        public Task RunAsync(string[] args)
        {
            return Task.CompletedTask;
        }
    }
}
