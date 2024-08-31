using DotMake.CommandLine;

namespace K8sOperator.NET.Commands;

[CliCommand(
    Description = "A root cli command with external children and one nested child and testing settings inheritance",
    NameCasingConvention = CliNameCasingConvention.SnakeCase,
    NamePrefixConvention = CliNamePrefixConvention.DoubleHyphen)]
public class Root
{
    public void Run(CliContext context)
    {
        if (!context.IsEmptyCommand())
        {
            context.ShowHelp();
        }        
    }
}
