using K8sOperator.NET.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace K8sOperator.NET.Builder;
public interface IOperatorApplicationBuilder
{
    IConfiguration Configuration { get; }
    IServiceCollection Services { get; }
    ILoggingBuilder Logging { get; }
    IControllerDataSource DataSource { get; }

    List<object> Metadata { get; }
    
    IOperatorApplication Build();
}

internal class OperatorApplicationBuilder : IOperatorApplicationBuilder, IControllerBuilder
{
    private readonly ServiceCollection _serviceCollection = new();
    private readonly ConfigurationManager _configurationManager = new();
    private readonly LoggingBuilder _logging;
    private readonly KubernetesBuilder _kubernetes;
    private readonly string[] _args;

    internal OperatorApplicationBuilder(string[] args)
    {
        _logging = new(_serviceCollection);
        _kubernetes = new(_serviceCollection);

        ConfigureConfiguration();
        ConfigureLogging();
        ConfigureKubernetes();
        _args = args;
    }

    public IConfiguration Configuration => _configurationManager;
    public IServiceCollection Services => _serviceCollection;
    public ILoggingBuilder Logging => _logging;
    public IKubernetesBuilder Kubernetes => _kubernetes;

    public IControllerDataSource DataSource { get; set; } = new ControllerDatasource();

    public List<object> Metadata { get; } = [];

    public IOperatorApplication Build()
    {
        _serviceCollection.AddSingleton(DataSource);

        var serviceProvider = _serviceCollection.BuildServiceProvider();

       return new OperatorHostApplication(serviceProvider, DataSource, _args);
    }

    private void ConfigureConfiguration()
    {
        _configurationManager.
            SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true, true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true, true)
            .AddEnvironmentVariables();
    }

    private void ConfigureLogging()
    {
        _serviceCollection.AddLogging(config =>
        {
            config.AddConfiguration(Configuration.GetSection("Logging"));
            config.AddConsole();
        });
    }

    private void ConfigureKubernetes()
    {
        _serviceCollection.AddKubernetes();
    }
}


internal sealed class LoggingBuilder(IServiceCollection services) : ILoggingBuilder
{
    public IServiceCollection Services { get; } = services;
}

internal sealed class KubernetesBuilder(IServiceCollection services) : IKubernetesBuilder
{
    public IServiceCollection Services { get; } = services;
}
