using k8s;
using K8sOperator.NET;
using K8sOperator.NET.Builder;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    public static IServiceCollection AddK8sOperators(this IServiceCollection services, Action<IOperatorConventionBuilder> config)
    {
        var conventions = new List<Action<IOperatorBuilder>>();
        var configBuilder = new OperatorConventionBuilder(conventions, []);

        config(configBuilder);

        services.AddSingleton<IKubernetes>((_) => SetupClient());

        services.AddSingleton<IOperatorBuilder>(s =>
        {
            var builder = new OperatorBuilder(s);
            conventions.ForEach(x => x(builder));
            return builder;
        });

        services.AddHostedService<Operator>();

        return services;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IOperatorConventionBuilder MapController<T>(this IHost builder)
        where T : IController
    {
        var ob = builder.Services.GetRequiredService<IOperatorBuilder>();
        return ob.MapController(typeof(T));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="controllerType"></param>
    /// <returns></returns>
    public static IOperatorConventionBuilder MapController(this IOperatorBuilder builder, Type controllerType)
    {
        return builder
            .GetOrAddDataSource()
            .AddController(controllerType)
            .WithMetaData([.. builder.MetaData]);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    private static IOperatorDataSource GetOrAddDataSource(this IOperatorBuilder builder)
    {
        builder.DataSource ??= new OperatorDatasource(builder.ServiceProvider);
        return builder.DataSource;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TBuilder"></typeparam>
    /// <param name="builder"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public static TBuilder RemoveMetaData<TBuilder>(this TBuilder builder, object item)
        where TBuilder : IOperatorConventionBuilder
    {
        builder.Add(b =>
        {
            b.MetaData.RemoveAll(x => x.GetType() == item.GetType());
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
    public static TBuilder WithMetaData<TBuilder>(this TBuilder builder, params object[] items)
        where TBuilder : IOperatorConventionBuilder
    {
        builder.Add(b =>
        {
            foreach (var item in items)
            {
                b.MetaData.Add(item);
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
        where TBuilder : IOperatorConventionBuilder
    {
        builder.RemoveMetaData(metadata);
        builder.WithMetaData(metadata);
        return builder;
    }


    private static Kubernetes SetupClient()
    {
        KubernetesClientConfiguration config;

        if (KubernetesClientConfiguration.IsInCluster())
        {
            config = KubernetesClientConfiguration.InClusterConfig();
        }
        else
        {
            config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
        }

        return new Kubernetes(config);
    }
}
