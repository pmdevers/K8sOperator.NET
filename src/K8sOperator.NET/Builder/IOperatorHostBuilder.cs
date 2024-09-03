using K8sOperator.NET.Extensions;
using K8sOperator.NET.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;


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
    private readonly List<object> _metadata = [];
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
        ConfigureMetadata();
        _args = args;

        DataSource = new ControllerDatasource(_metadata);
    }

    private void ConfigureMetadata()
    {
        
        var operatorName = Assembly.GetEntryAssembly()?.GetCustomAttribute<OperatorNameAttribute>()
            ?? new OperatorNameAttribute("operator");
        
        _metadata.Add(operatorName);

        var dockerImage = Assembly.GetEntryAssembly()?.GetCustomAttribute<DockerImageAttribute>() 
            ?? new DockerImageAttribute("ghcr.io", "operator", "operator", "latest");

        _metadata.Add(dockerImage);
    }

    public IConfiguration Configuration => _configurationManager;
    public IServiceCollection Services => _serviceCollection;
    public ILoggingBuilder Logging => _logging;
    public IKubernetesBuilder Kubernetes => _kubernetes;

    public IControllerDataSource DataSource { get; set; }

    public List<object> Metadata => _metadata;

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
