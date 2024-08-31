using K8sOperator.NET.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace K8sOperator.NET;

public interface IOperatorApplication
{
    IServiceProvider ServiceProvider { get; }
    IControllerDataSource DataSource { get; }
    Task RunAsync();
}

/// <summary>
/// 
/// </summary>
public static class OperatorHost
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static IOperatorApplicationBuilder CreateOperatorApplicationBuilder(params string[] args)
        => new OperatorApplicationBuilder(args);
}

/// <summary>
/// 
/// </summary>
public partial class OperatorHostApplication : IOperatorApplication
{
    internal OperatorHostApplication(IServiceProvider serviceProvider, IControllerDataSource dataSource)
    {
        ServiceProvider = serviceProvider;
        DataSource = dataSource;
    }

    /// <summary>
    /// 
    /// </summary>
    public IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// 
    /// </summary>
    public IControllerDataSource DataSource { get; }
    
    public async Task RunAsync()
    {
        var ops = ActivatorUtilities.CreateInstance<Operator>(ServiceProvider, DataSource);
        await ops.RunAsync();
    }
}
