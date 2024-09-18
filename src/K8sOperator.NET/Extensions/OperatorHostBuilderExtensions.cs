using K8sOperator.NET;
using K8sOperator.NET.Builder;
using K8sOperator.NET.Metadata;

namespace K8sOperator.NET.Extensions;

/// <summary>
/// Provides extension methods for configuring an operator host.
/// </summary>
public static class OperatorHostBuilderExtensions
{

    /// <summary>
    /// Sets the name of the operator application.
    /// </summary>
    /// <param name="builder">The operator application builder.</param>
    /// <param name="name">The name of the operator.</param>
    /// <returns>The configured operator application builder.</returns>
    public static IOperatorApplicationBuilder WithName(this IOperatorApplicationBuilder builder,
        string name
        )
    {
        builder.Metadata.RemoveAll(x => x.GetType() == typeof(OperatorNameAttribute));
        builder.Metadata.Add(new OperatorNameAttribute(name.ToLowerInvariant()));
        return builder;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="ns"></param>
    /// <returns></returns>
    public static IOperatorApplicationBuilder WithNamespace(this IOperatorApplicationBuilder builder, string ns)
    {
        builder.Metadata.RemoveAll(x => x is NamespaceAttribute);
        builder.Metadata.Add(new NamespaceAttribute(ns.ToLowerInvariant()));
        return builder;
    }

    /// <summary>
    /// Sets the Docker image information for the operator application.
    /// </summary>
    /// <param name="builder">The operator application builder.</param>
    /// <param name="registery">The Docker registry. Defaults to "ghcr.io".</param>
    /// <param name="repository">The Docker repository.</param>
    /// <param name="tag">The tag of the Docker image.</param>
    /// <returns>The configured operator application builder.</returns>
    public static IOperatorApplicationBuilder WithImage(this IOperatorApplicationBuilder builder,
        string registery = "ghcr.io",
        string repository = "",
        string tag = ""
        )
    {
        builder.Metadata.RemoveAll(x => x.GetType() == typeof(DockerImageAttribute));
        builder.Metadata.Add(new DockerImageAttribute(registery, repository, tag));
        return builder;
    }

    /// <summary>
    /// Adds a controller to the operator application.
    /// </summary>
    /// <typeparam name="T">The type of the controller to add.</typeparam>
    /// <param name="builder">The operator application builder.</param>
    /// <returns>The controller convention builder for further configuration.</returns>
    public static IControllerConventionBuilder AddController<T>(this IOperatorApplicationBuilder builder)
        where T : IController
        => builder.AddController(typeof(T));

}
