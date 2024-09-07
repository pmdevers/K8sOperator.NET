namespace K8sOperator.NET.Helpers;
internal static class ConsoleHelpers
{
    internal static readonly string SPACE = string.Empty.PadRight(3);
    internal static readonly string NL = Environment.NewLine; // shortcut
    internal static readonly string NORMAL = Console.IsOutputRedirected ? "" : "\x1b[39m";
    internal static readonly string RED = Console.IsOutputRedirected ? "" : "\x1b[91m";
    internal static readonly string GREEN = Console.IsOutputRedirected ? "" : "\x1b[92m";
    internal static readonly string YELLOW = Console.IsOutputRedirected ? "" : "\x1b[93m";
    internal static readonly string BLUE = Console.IsOutputRedirected ? "" : "\x1b[94m";
    internal static readonly string MAGENTA = Console.IsOutputRedirected ? "" : "\x1b[95m";
    internal static readonly string CYAN = Console.IsOutputRedirected ? "" : "\x1b[96m";
    internal static readonly string GREY = Console.IsOutputRedirected ? "" : "\x1b[97m";
    internal static readonly string BOLD = Console.IsOutputRedirected ? "" : "\x1b[1m";
    internal static readonly string NOBOLD = Console.IsOutputRedirected ? "" : "\x1b[22m";
    internal static readonly string UNDERLINE = Console.IsOutputRedirected ? "" : "\x1b[4m";
    internal static readonly string NOUNDERLINE = Console.IsOutputRedirected ? "" : "\x1b[24m";
    internal static readonly string REVERSE = Console.IsOutputRedirected ? "" : "\x1b[7m";
    internal static readonly string NOREVERSE = Console.IsOutputRedirected ? "" : "\x1b[27m";
}
