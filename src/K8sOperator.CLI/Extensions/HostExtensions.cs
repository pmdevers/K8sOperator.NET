using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;


namespace K8sOperator.CLI.Extensions;
/// <summary>
/// 
/// </summary>
public static class HostExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static Task OperatorAsync(this IHost app)
    {
        var rootCommand = new RootCommand()
        {
            new Option<bool>("--operator", "Run the operator")
        };

        rootCommand.Handler = CommandHandler.Create<bool>(async (oper) =>
        {
            if(oper)
            {
                await app.RunAsync();
            } else
            {
                Console.WriteLine("cli");
            }
        });
        

        return rootCommand.InvokeAsync(Environment.GetCommandLineArgs());
    }
}
