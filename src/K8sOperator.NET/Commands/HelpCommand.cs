using K8sOperator.NET.Builder;
using K8sOperator.NET.Metadata;
using static K8sOperator.NET.Helpers.ConsoleHelpers;

namespace K8sOperator.NET.Commands;

[OperatorArgument("help", Description = "Displays all commands.", Order = 999)]
internal partial class HelpCommand(IOperatorApplication app) : IOperatorCommand
{
    public Task RunAsync(string[] args)
    {
        var commands = app.Commands.GetCommands();
        var list = commands.SelectMany(x => x.Metadata.OfType<ICommandArgumentMetadata>()).ToList();
        var maxSize = list.Max(x => x.Argument.Length);

        Console.WriteLine($"Welcome to the help for {RED}{app.Name}{NORMAL}.");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine($"{BOLD}{app.Name}{NOBOLD} [command]");
        Console.WriteLine();
        Console.WriteLine($"{BOLD}Available Commands:{NOBOLD}");
        foreach (var argument in list)
        {
            Console.WriteLine($"{SPACE}{GREEN}{argument.Argument.PadRight(maxSize)}{NORMAL}{SPACE}{YELLOW}{argument.Description}{NORMAL}");
        }
        Console.WriteLine();

        return Task.CompletedTask;
    }
}
