using K8sOperator.NET.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using static K8sOperator.NET.Helpers.ConsoleHelpers;

namespace K8sOperator.NET.Commands;

[OperatorArgument("help", Description = "Displays all commands", Order = 999)]
public class HelpCommand(IHost app) : IOperatorCommand
{
    public Task RunAsync(string[] args)
    {
        var appname = AppDomain.CurrentDomain.FriendlyName;
        var commandDatasource = app.Services.GetRequiredService<CommandDatasource>();
        var commands = commandDatasource.GetCommands(app);
        var info = commands.SelectMany(x => x.Metadata.OfType<OperatorArgumentAttribute>()).ToList();
        var maxSize = info.Count > 0 ? info.Max(x => x.Argument.Length) : 0;

        Console.WriteLine($"Welcome to the help for {RED}{appname}{NORMAL}.");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine($"{BOLD}{appname}{NOBOLD} [command]");
        Console.WriteLine();
        Console.WriteLine($"{BOLD}Available Commands:{NOBOLD}");
        foreach (var argument in info.Where(x => x.ShowInHelp))
        {
            Console.WriteLine($"{SPACE}{GREEN}{argument.Argument.PadRight(maxSize)}{NORMAL}{SPACE}{YELLOW}{argument.Description}{NORMAL}");
        }
        Console.WriteLine();

        return Task.CompletedTask;
    }
}
