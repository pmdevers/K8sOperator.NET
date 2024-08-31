using K8sOperator.NET;
using K8sOperator.NET.Builder;
using K8sOperator.NET.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 
/// </summary>
public static class OperatorHostBuilderExtensions
{
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
