using K8sOperator.NET.Metadata;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace K8sOperator.NET.Configuration;

/// <summary>
/// Provides operator configuration from multiple sources with priority:
/// 1. OperatorBuilder (explicit configuration)
/// 2. appsettings.json / IConfiguration
/// 3. Assembly attributes (build-time metadata)
/// 4. Defaults
/// </summary>
public class OperatorConfigurationProvider(
    IConfiguration? configuration = null,
    Assembly? assembly = null)
{
    private readonly Assembly _assembly = assembly
        ?? Assembly.GetEntryAssembly()
        ?? Assembly.GetExecutingAssembly();

    /// <summary>
    /// Build operator configuration from all available sources
    /// </summary>
    public OperatorConfiguration Build()
    {
        var config = new OperatorConfiguration();

        // 1. Start with assembly attributes (lowest priority)
        ApplyAssemblyAttributes(config);

        // 2. Apply configuration (e.g., appsettings.json)
        ApplyConfiguration(config);

        // 3. OperatorBuilder can override in AddOperator() (highest priority, done by caller)

        return config;
    }

    private void ApplyAssemblyAttributes(OperatorConfiguration config)
    {
        // Read OperatorName from assembly attribute
        var operatorNameAttr = _assembly.GetCustomAttribute<OperatorNameAttribute>();
        if (operatorNameAttr != null && !string.IsNullOrEmpty(operatorNameAttr.OperatorName))
        {
            config.OperatorName = operatorNameAttr.OperatorName;
        }

        // Read Namespace from assembly attribute
        var namespaceAttr = _assembly.GetCustomAttribute<NamespaceAttribute>();
        if (namespaceAttr != null && !string.IsNullOrEmpty(namespaceAttr.Namespace))
        {
            config.Namespace = namespaceAttr.Namespace;
        }

        // Read Docker image from assembly attribute
        var dockerAttr = _assembly.GetCustomAttribute<DockerImageAttribute>();
        if (dockerAttr != null)
        {
            if (!string.IsNullOrEmpty(dockerAttr.Registry))
                config.ContainerRegistry = dockerAttr.Registry;

            if (!string.IsNullOrEmpty(dockerAttr.Repository))
                config.ContainerRepository = dockerAttr.Repository;

            if (!string.IsNullOrEmpty(dockerAttr.Tag))
                config.ContainerTag = dockerAttr.Tag;
        }
    }

    private void ApplyConfiguration(OperatorConfiguration config)
    {
        if (configuration == null)
            return;

        // Bind from appsettings.json section: "Operator"
        var section = configuration.GetSection("Operator");
        if (section.Exists())
        {
            section.Bind(config);
        }
    }
}
