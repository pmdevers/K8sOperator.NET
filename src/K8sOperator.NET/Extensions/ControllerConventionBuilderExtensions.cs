using K8sOperator.NET.Builder;

namespace K8sOperator.NET.Extensions;

/// <summary>
/// 
/// </summary>
public static class ControllerConventionBuilderExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TBuilder"></typeparam>
    /// <param name="builder"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public static TBuilder RemoveMetadata<TBuilder>(this TBuilder builder, object item)
        where TBuilder : IControllerConventionBuilder
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
        where TBuilder : IControllerConventionBuilder
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
        where TBuilder : IControllerConventionBuilder
    {
        builder.RemoveMetadata(metadata);
        builder.WithMetadata(metadata);
        return builder;
    }
}



/// <summary>
/// 
/// </summary>
public static class OperatorCommandConventionBuilderExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TBuilder"></typeparam>
    /// <param name="builder"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public static TBuilder RemoveMetadata<TBuilder>(this TBuilder builder, object item)
        where TBuilder : IOperatorCommandConventionBuilder
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
        where TBuilder : IOperatorCommandConventionBuilder
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
        where TBuilder : IOperatorCommandConventionBuilder
    {
        builder.RemoveMetadata(metadata);
        builder.WithMetadata(metadata);
        return builder;
    }
}
