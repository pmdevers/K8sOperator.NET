using K8sOperator.NET.Builder;
using K8sOperator.NET.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace K8sOperator.NET.Extensions;

/// <summary>
/// 
/// </summary>
public static class MetadataExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TBuilder"></typeparam>
    /// <param name="builder"></param>
    /// <param name="watchNamespace"></param>
    /// <returns></returns>
    public static TBuilder WatchNamespace<TBuilder>(this TBuilder builder, string watchNamespace)
        where TBuilder : IOperatorConventionBuilder
    {
        builder.WithSingle(new WatchNamespaceMetadata(watchNamespace));
        return builder;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TBuilder"></typeparam>
    /// <param name="builder"></param>
    /// <param name="labelselector"></param>
    /// <returns></returns>
    public static TBuilder WithLabel<TBuilder>(this TBuilder builder, string labelselector)
        where TBuilder : IOperatorConventionBuilder
    {
        builder.WithSingle(new LabelSelectorMetadata(labelselector));
        return builder;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TBuilder"></typeparam>
    /// <param name="builder"></param>
    /// <param name="finalizer"></param>
    /// <returns></returns>
    public static TBuilder WithFinalizer<TBuilder>(this TBuilder builder, string finalizer)
        where TBuilder : IOperatorConventionBuilder
    {
        builder.WithSingle(new FinalizerMetadata(finalizer));
        return builder;
    }
}
