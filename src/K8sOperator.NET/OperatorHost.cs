﻿using K8sOperator.NET.Builder;
using K8sOperator.NET.Commands;
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
    private readonly string[] _args;

    internal OperatorHostApplication(IServiceProvider serviceProvider, IControllerDataSource dataSource, string[] args)
    {
        ServiceProvider = serviceProvider;
        DataSource = dataSource;
        _args = args;
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
        if (_args.Contains("--operator"))
        {
            var oper = ActivatorUtilities.CreateInstance<Operator>(ServiceProvider, DataSource);
            await oper.RunAsync();
        } 
        else if(_args.Contains("install"))
        {
            var installer = ActivatorUtilities.CreateInstance<Install>(ServiceProvider, DataSource);
            await installer.RunAsync();
        }
    }
}
