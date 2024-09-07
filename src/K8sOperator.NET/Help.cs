using K8sOperator.NET.Builder;
using K8sOperator.NET.Metadata;

namespace K8sOperator.NET;

[OperatorArgument("help", Description = "Displays all commands.")]
internal partial class Help(IOperatorApplication app) : IOperatorCommand
{
    public Task RunAsync(string[] args)
    {
        var commands = app.Commands.GetCommands();
        var list = commands.SelectMany(x =>x.Metadata.OfType<ICommandArgumentMetadata>()).ToList();
        var maxSize = list.Max(x => x.Argument.Length);

        Console.WriteLine($"Welcome to the help for {RED}{AppDomain.CurrentDomain.FriendlyName}{NORMAL}.");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine($"{BOLD}{AppDomain.CurrentDomain.FriendlyName}{NOBOLD} [command]");
        Console.WriteLine();
        Console.WriteLine($"{BOLD}Available Commands:{NOBOLD}");
        foreach (var argument in list) {
            Console.WriteLine($"{SPACE}{GREEN}{argument.Argument.PadRight(maxSize)}{NORMAL}{SPACE}{YELLOW}{argument.Description}{NORMAL}");
        }
        Console.WriteLine();

        return Task.CompletedTask;
    }

#pragma warning disable S1144 // Unused private types or members should be removed
    private static readonly string SPACE = string.Empty.PadRight(3);
    private static readonly string NL = Environment.NewLine; // shortcut
    private static readonly string NORMAL = Console.IsOutputRedirected ? "" : "\x1b[39m";
    private static readonly string RED = Console.IsOutputRedirected ? "" : "\x1b[91m";
    private static readonly string GREEN = Console.IsOutputRedirected ? "" : "\x1b[92m";
    private static readonly string YELLOW = Console.IsOutputRedirected ? "" : "\x1b[93m";
    private static readonly string BLUE = Console.IsOutputRedirected ? "" : "\x1b[94m";
    private static readonly string MAGENTA = Console.IsOutputRedirected ? "" : "\x1b[95m";
    private static readonly string CYAN = Console.IsOutputRedirected ? "" : "\x1b[96m";
    private static readonly string GREY = Console.IsOutputRedirected ? "" : "\x1b[97m";
    private static readonly string BOLD = Console.IsOutputRedirected ? "" : "\x1b[1m";
    private static readonly string NOBOLD = Console.IsOutputRedirected ? "" : "\x1b[22m";
    private static readonly string UNDERLINE = Console.IsOutputRedirected ? "" : "\x1b[4m";
    private static readonly string NOUNDERLINE = Console.IsOutputRedirected ? "" : "\x1b[24m";
    private static readonly string REVERSE = Console.IsOutputRedirected ? "" : "\x1b[7m";
    private static readonly string NOREVERSE = Console.IsOutputRedirected ? "" : "\x1b[27m";

#pragma warning restore S1144 // Unused private types or members should be removed
}
