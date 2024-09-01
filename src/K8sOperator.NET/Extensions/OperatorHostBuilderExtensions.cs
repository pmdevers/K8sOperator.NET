using K8sOperator.NET;
using K8sOperator.NET.Builder;
using K8sOperator.NET.Extensions;
using K8sOperator.NET.Metadata;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 
/// </summary>
public static class OperatorHostBuilderExtensions
{
    public static IOperatorApplicationBuilder WithName(this IOperatorApplicationBuilder builder,
        string name
        )
    {
        builder.Metadata.RemoveAll(x => x.GetType() == typeof(OperatorNameMetadata));
        builder.Metadata.Add(new OperatorNameMetadata(name.ToLowerInvariant()));
        return builder;
    }

    public static IOperatorApplicationBuilder WithImage(this IOperatorApplicationBuilder builder,
        string registery = "ghcr.io",
        string repository = "",
        string name = "",
        string tag = ""
        )
    {
        builder.Metadata.RemoveAll(x => x.GetType() == typeof(ImageMetadata));
        builder.Metadata.Add(new ImageMetadata(registery, repository, name, tag));
        return builder;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IControllerConventionBuilder AddController<T>(this IOperatorApplicationBuilder builder)
        where T : IController
        => builder.AddController(typeof(T));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="controllerType"></param>
    /// <returns></returns>
    public static IControllerConventionBuilder AddController(this IOperatorApplicationBuilder builder, Type controllerType)
    {
        return builder
            .DataSource
            .AddController(controllerType)
            .WithMetadata([.. builder.Metadata]);
    }
}
