using K8sOperator.NET.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace K8sOperator.NET;

/// <summary>
/// Interface representing an operator application that manages Kubernetes resources.
/// </summary>
public interface IOperatorApplication
{
    /// <summary>
    /// Gets the service provider that is used to resolve dependencies within the application.
    /// </summary>
    IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// The application's configured Microsoft.Extensions.Configuration.IConfiguration
    /// </summary>
    IConfiguration Configuration { get; }

    /// <summary>
    /// Gets the data source that provides access to Kubernetes controllers.
    /// </summary>
    IControllerDataSource DataSource { get; }

    /// <summary>
    /// Runs the operator application asynchronously, managing the lifecycle of Kubernetes resources.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RunAsync();
}

/// <summary>
/// Provides functionality to create and configure an operator application.
/// </summary>
public static class OperatorHost
{
    /// <summary>
    /// Creates an <see cref="IOperatorApplicationBuilder"/> used to configure and build an operator application.
    /// </summary>
    /// <param name="args">The command-line arguments passed to the application.</param>
    /// <returns>An instance of <see cref="IOperatorApplicationBuilder"/> for further configuration.</returns>
    public static IOperatorApplicationBuilder CreateOperatorApplicationBuilder(params string[] args)
        => new OperatorApplicationBuilder(args);
}

internal class OperatorHostApplication : IOperatorApplication
{
    internal OperatorHostApplication(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        IControllerDataSource dataSource
    )
    {
        ServiceProvider = serviceProvider;
        Configuration = configuration;
        DataSource = dataSource;
    }

    public IServiceProvider ServiceProvider { get; }

    public IConfiguration Configuration { get; }

    public IControllerDataSource DataSource { get; }

    public async Task RunAsync()
    {
        var oper = ActivatorUtilities.CreateInstance<Operator>(ServiceProvider, DataSource);
        await oper.RunAsync();
    }
}
