using k8s;
using K8sOperator.NET;
using K8sOperator.NET.Tests.Mocks;

namespace K8sOperator.NET.Tests;

public class OperatorExtensions_Tests
{
    [Test]
    public async Task AddOperator_NullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;
        // Act
        var ex = Assert.Throws<ArgumentNullException>(() => services.AddOperator());
        // Assert
        await Assert.That(ex.ParamName).IsEqualTo("services");
    }

    [Test]
    public async Task AddOperator_ValidServices_AddsOperatorService()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        // Act
        services.AddOperator();
        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var hostedServices = serviceProvider.GetServices<IHostedService>();

        await Assert.That(hostedServices).HasSingleItem();
    }

    [Test]
    public async Task AddOperator_ValidServices_RegistersCommandDatasource()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        // Act
        services.AddOperator();
        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var commandDatasource = serviceProvider.GetService<CommandDatasource>();

        await Assert.That(commandDatasource).IsNotNull();
    }

    [Test]
    public async Task AddOperator_ValidServices_RegistersEventWatcherDatasource()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        // Act
        services.AddOperator();
        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var eventWatcherDatasource = serviceProvider.GetService<EventWatcherDatasource>();
        await Assert.That(eventWatcherDatasource).IsNotNull();
    }

    [Test]
    public async Task AddOperator_ValidServices_RegistersKubernetesClient()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        // Act
        services.AddOperator();
        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var kubernetesClient = serviceProvider.GetService<IKubernetes>();
        await Assert.That(kubernetesClient).IsNotNull();
    }

    [Test]
    public async Task AddOperator_WithConfigure_CallsConfigureAction()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        var configureCalled = false;
        // Act
        services.AddOperator(builder => configureCalled = true);
        // Assert
        await Assert.That(configureCalled).IsTrue();
    }

    private readonly TestContext _context;

    [Test]
    public async Task AddOperator_ValidServices_RegistersDefaultCommands()
    {
        using var server = new MockKubeApiServer(_context);

        // Assert
        var host = new HostBuilder()
            .ConfigureServices(s =>
            {
                s.AddSingleton(new KubernetesClientConfiguration
                {
                    Host = server.Uri.ToString()
                });
                s.AddOperator();
            })
            .Build();



        var commandDatasource = host.Services.GetRequiredService<CommandDatasource>();
        var commands = commandDatasource.GetCommands(host);

        await Assert.That(commands).Count().IsEqualTo(4);
    }

    [Test]
    public async Task AddOperator_ReturnsSameServiceCollection()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        // Act
        var result = services.AddOperator();
        // Assert
        await Assert.That(result).IsEqualTo(services);
    }
}
