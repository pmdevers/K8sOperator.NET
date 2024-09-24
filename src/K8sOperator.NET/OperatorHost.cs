using K8sOperator.NET.Builder;
using K8sOperator.NET.Commands;
using K8sOperator.NET.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace K8sOperator.NET;

/// <summary>
/// Interface representing an operator application that manages Kubernetes resources.
/// </summary>
public interface IOperatorApplication
{
    /// <summary>
    /// The name of the operator.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The version number of the operator
    /// </summary>
    public string Version { get; }

    /// <summary>
    /// Gets the service provider that is used to resolve dependencies within the application.
    /// </summary>
    IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// The application's configured Microsoft.Extensions.Configuration.IConfiguration
    /// </summary>
    IConfiguration Configuration { get; }

    /// <summary>
    /// The application's configured Logger Factory
    /// </summary>
    ILoggerFactory Logger { get; }

    /// <summary>
    /// Gets the data source that provides access to Kubernetes controllers.
    /// </summary>
    IControllerDataSource DataSource { get; }

    /// <summary>
    /// 
    /// </summary>
    ICommandDatasource Commands { get; }

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
    private readonly string[] _args;

    internal OperatorHostApplication(
        IServiceProvider serviceProvider,
        string[] args
    )
    {
        ServiceProvider = serviceProvider;
        Configuration = ServiceProvider.GetRequiredService<IConfiguration>();
        DataSource = ServiceProvider.GetRequiredService<IControllerDataSource>();
        Logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
        Commands = new CommandDatasource(serviceProvider);
        
        Commands.AddCommand(typeof(OperatorCommand));
        Commands.AddCommand(typeof(VersionCommand));
        Commands.AddCommand(typeof(HelpCommand));

        _args = args;
    }

    public IServiceProvider ServiceProvider { get; }

    public IConfiguration Configuration { get; }

    public IControllerDataSource DataSource { get; }

    public ICommandDatasource Commands { get; }

    public ILoggerFactory Logger { get; }

    public string Name => DataSource.Metadata.OfType<IOperatorNameMetadata>().First().OperatorName;
    public string Version => DataSource.Metadata.OfType<IImageMetadata>().First().Tag;

    public async Task RunAsync()
    {
        var commands = Commands.GetCommands();
        var command = commands
            .FirstOrDefault(Filter)
            ?.Command;

        if(command == null) 
        { 
            await new HelpCommand(this).RunAsync([.._args]);
            return;
        }

        await command.RunAsync(_args);
    }

    private bool Filter(CommandInfo command)
    {
        var arg = command.Metadata.OfType<ICommandArgumentMetadata>().First().Argument;
        return _args.FirstOrDefault() == arg || _args.FirstOrDefault() == $"--{arg}";
    }
}
