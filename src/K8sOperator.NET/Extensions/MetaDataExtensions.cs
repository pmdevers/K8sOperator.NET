using K8sOperator.NET.Builder;
using K8sOperator.NET.Metadata;

namespace K8sOperator.NET.Extensions;

/// <summary>
/// 
/// </summary>
public static class MetadataExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="metaData"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    public static T2? TryGetValue<T, T2>(this IReadOnlyList<object> metaData, Func<T, T2> selector)
    {
        var type = metaData.OfType<T>().FirstOrDefault();
        return type is null ? default : selector(type);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TBuilder"></typeparam>
    /// <param name="builder"></param>
    /// <param name="group"></param>
    /// <returns></returns>
    public static TBuilder WithGroup<TBuilder>(this TBuilder builder, string group)
        where TBuilder : IControllerConventionBuilder
    {
        builder.WithSingle(new GroupMetadata(group));
        return builder;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TBuilder"></typeparam>
    /// <param name="builder"></param>
    /// <param name="kind"></param>
    /// <returns></returns>
    public static TBuilder WithKind<TBuilder>(this TBuilder builder, string kind)
        where TBuilder : IControllerConventionBuilder
    {
        builder.WithSingle(new KindMetadata(kind));
        return builder;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TBuilder"></typeparam>
    /// <param name="builder"></param>
    /// <param name="version"></param>
    /// <returns></returns>
    public static TBuilder WithVersion<TBuilder>(this TBuilder builder, string version)
        where TBuilder : IControllerConventionBuilder
    {
        builder.WithSingle(new ApiVersionMetadata(version));
        return builder;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TBuilder"></typeparam>
    /// <param name="builder"></param>
    /// <param name="pluralName"></param>
    /// <returns></returns>
    public static TBuilder WithPluralName<TBuilder>(this TBuilder builder, string pluralName)
        where TBuilder : IControllerConventionBuilder
    {
        builder.WithSingle(new PluralNameMetadata(pluralName));
        return builder;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TBuilder"></typeparam>
    /// <param name="builder"></param>
    /// <param name="watchNamespace"></param>
    /// <returns></returns>
    public static TBuilder WatchNamespace<TBuilder>(this TBuilder builder, string watchNamespace)
        where TBuilder : IControllerConventionBuilder
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
        where TBuilder : IControllerConventionBuilder
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
        where TBuilder : IControllerConventionBuilder
    {
        builder.WithMetadata(new FinalizerMetadata(finalizer));
        return builder;
    }
}
