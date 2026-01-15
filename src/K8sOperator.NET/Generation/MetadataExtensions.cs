using k8s.Models;
using K8sOperator.NET.Builder;
using K8sOperator.NET.Generation;
using K8sOperator.NET.Metadata;

namespace K8sOperator.NET.Generation;

/// <summary>
/// Provides extension methods for metadata manipulation and configuration of Kubernetes controllers.
/// </summary>
public static class MetadataExtensions
{
    /// <summary>
    /// Tries to get a value from the metadata collection based on the specified type and selector function.
    /// </summary>
    /// <typeparam name="T">The type to search for in the metadata collection.</typeparam>
    /// <typeparam name="T2">The type of the value to return.</typeparam>
    /// <param name="metaData">The metadata collection.</param>
    /// <param name="selector">The selector function to apply if the type is found.</param>
    /// <returns>The selected value if the type is found; otherwise, the default value of <typeparamref name="T2"/>.</returns>
    public static T2? TryGetValue<T, T2>(this IReadOnlyList<object> metaData, Func<T, T2> selector)
    {
        var type = metaData.OfType<T>().FirstOrDefault();
        return type is null ? default : selector(type);
    }

    /// <summary>
    /// Configures the builder with a Kubernetes entity group, version, kind, and plural name.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder to configure.</param>
    /// <param name="group">The Kubernetes API group.</param>
    /// <param name="version">The API version. Defaults to "v1".</param>
    /// <param name="kind">The kind of the Kubernetes entity.</param>
    /// <param name="pluralName">The plural name of the Kubernetes entity.</param>
    /// <returns>The configured builder.</returns>
    public static TBuilder WithGroup<TBuilder>(this TBuilder builder,
        string group = "",
        string version = "v1",
        string kind = "",
        string pluralName = ""
        )
        where TBuilder : ConventionBuilder<ControllerBuilder>
    {
        builder.WithSingle(new KubernetesEntityAttribute()
        {
            Group = group,
            ApiVersion = version,
            Kind = kind,
            PluralName = pluralName
        });
        return builder;
    }

    /// <summary>
    /// Configures the builder to watch a specific namespace.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder to configure.</param>
    /// <param name="watchNamespace">The namespace to watch.</param>
    /// <returns>The configured builder.</returns>
    public static TBuilder ForNamespace<TBuilder>(this TBuilder builder, string watchNamespace)
        where TBuilder : ConventionBuilder<ControllerBuilder>
    {
        builder.WithSingle(new NamespaceAttribute(watchNamespace));
        return builder;
    }

    /// <summary>
    /// Configures the builder with a label selector for filtering Kubernetes resources.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder to configure.</param>
    /// <param name="labelselector">The label selector string.</param>
    /// <returns>The configured builder.</returns>
    public static TBuilder WithLabel<TBuilder>(this TBuilder builder, string labelselector)
        where TBuilder : ConventionBuilder<ControllerBuilder>
    {
        builder.WithSingle(new LabelSelectorAttribute(labelselector));
        return builder;
    }

    /// <summary>
    /// Configures the builder with a finalizer for the Kubernetes resource.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder to configure.</param>
    /// <param name="finalizer">The finalizer string.</param>
    /// <returns>The configured builder.</returns>
    public static TBuilder WithFinalizer<TBuilder>(this TBuilder builder, string finalizer)
        where TBuilder : ConventionBuilder<ControllerBuilder>
    {
        builder.WithMetadata(new FinalizerAttribute(finalizer));
        return builder;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TBuilder"></typeparam>
    /// <param name="builder"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public static TBuilder RemoveMetadata<TBuilder>(this TBuilder builder, object item)
        where TBuilder : ConventionBuilder<ControllerBuilder>
    {
        builder.Add(b =>
        {
            b.Metadata.RemoveAll(x => x.GetType() == item.GetType());
        });

        return builder;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TBuilder"></typeparam>
    /// <param name="builder"></param>
    /// <param name="items"></param>
    /// <returns></returns>
    public static TBuilder WithMetadata<TBuilder>(this TBuilder builder, params object[] items)
        where TBuilder : ConventionBuilder<ControllerBuilder>
    {
        builder.Add(b =>
        {
            foreach (var item in items)
            {
                b.Metadata.Add(item);
            }
        });

        return builder;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TBuilder"></typeparam>
    /// <param name="builder"></param>
    /// <param name="metadata"></param>
    /// <returns></returns>
    public static TBuilder WithSingle<TBuilder>(this TBuilder builder, object metadata)
        where TBuilder : ConventionBuilder<ControllerBuilder>
    {
        builder.RemoveMetadata(metadata);
        builder.WithMetadata(metadata);
        return builder;
    }
}
