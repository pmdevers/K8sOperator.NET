using K8sOperator.NET.Builder;

namespace K8sOperator.NET.Generators;

/// <summary>
/// 
/// </summary>
public static class OperatorApplicationExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TBuilder"></typeparam>
    /// <param name="builder"></param>
    /// <param name="order"></param>
    /// <returns></returns>
    public static IOperatorCommandConventionBuilder AddInstall<TBuilder>(this TBuilder builder, int? order = null)
        where TBuilder : IOperatorApplication
    {
        return builder.AddCommand<InstallCommand>(order ?? -1);
    }
}
