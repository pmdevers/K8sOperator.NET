using K8sOperator.NET.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace K8sOperator.NET.Extensions;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddK8sOperators(this IServiceCollection services, Action<IOperatorConventionBuilder> config)
    {
        var conventions = new List<Action<IOperatorBuilder>>();
        var configBuilder = new OperatorConventionBuilder(conventions, []);

        config(configBuilder);

        

        services.AddSingleton<IOperatorBuilder>(s =>
        {
            var builder = new OperatorBuilder(s);


        });
        services.AddHostedService<OperatorService>();
    }
}
