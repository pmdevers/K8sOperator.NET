﻿using K8sOperator.NET.Extensions;
using K8sOperator.NET.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace K8sOperator.NET.Builder;

/// <summary>
/// Interface for building an Operator Application.
/// </summary>
public interface IOperatorApplicationBuilder
{
    /// <summary>
    /// Gets the configuration settings for the application.
    /// </summary>
    IConfiguration Configuration { get; }

    /// <summary>
    /// Gets the collection of services used by the application.
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    /// Gets the logging builder for configuring logging services.
    /// </summary>
    ILoggingBuilder Logging { get; }

    /// <summary>
    /// Gets the data source for the controller, providing access to the Kubernetes resources.
    /// </summary>
    IControllerDataSource DataSource { get; }

    /// <summary>
    /// Gets the list of metadata associated with the application.
    /// </summary>
    List<object> Metadata { get; }

    /// <summary>
    /// Builds the Operator Application based on the configured settings and services.
    /// </summary>
    /// <returns>An instance of <see cref="IOperatorApplication"/> representing the built application.</returns>

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
