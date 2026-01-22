using K8sOperator.NET.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;
using System.Text.Json;

namespace K8sOperator.NET.Commands;

[OperatorArgument("generate-launchsettings", Description = "Generates launchSettings.json based on registered commands", Order = 100, ShowInHelp = false)]
public class GenerateLaunchSettingsCommand(IHost host) : IOperatorCommand
{
    public async Task RunAsync(string[] args)
    {
        var commandDatasource = host.Services.GetRequiredService<CommandDatasource>();
        var commands = commandDatasource.GetCommands(host);

        var launchSettings = new
        {
            profiles = commands
                .Where(c => c.Metadata.OfType<OperatorArgumentAttribute>().Any())
                .ToDictionary(
                    c => ToPascalCase(c.Metadata.OfType<OperatorArgumentAttribute>().First().Argument),
                    c => new
                    {
                        commandName = "Project",
                        commandLineArgs = c.Metadata.OfType<OperatorArgumentAttribute>().First().Argument,
                        environmentVariables = new Dictionary<string, string>
                        {
                            ["ASPNETCORE_ENVIRONMENT"] = "Development"
                        },
                        dotnetRunMessages = true
                    }
                ),
            schema = "http://json.schemastore.org/launchsettings.json"
        };

        string json = JsonSerializer.Serialize(launchSettings, new JsonSerializerOptions()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = null // Don't change dictionary keys
        });

        var propertiesDir = Path.Combine(Directory.GetCurrentDirectory(), "Properties");
        Directory.CreateDirectory(propertiesDir);

        var launchSettingsPath = Path.Combine(propertiesDir, "launchSettings.json");
        await File.WriteAllTextAsync(launchSettingsPath, json);

        Console.WriteLine($"✅ Generated launchSettings.json at: {launchSettingsPath}");
        Console.WriteLine($"   Found {commands.Count()} command(s):");

        foreach (var cmd in commands)
        {
            var arg = cmd.Metadata.OfType<OperatorArgumentAttribute>().FirstOrDefault();
            if (arg != null)
            {
                Console.WriteLine($"   - {arg.Argument}");
            }
        }
    }

    private static string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var parts = input.Split('-', '_');
        var sb = new StringBuilder();

        foreach (var part in parts)
        {
            if (!string.IsNullOrEmpty(part))
            {
                sb.Append(char.ToUpper(part[0]));
                if (part.Length > 1)
                    sb.Append(part[1..].ToLower());
            }
        }

        return sb.ToString();
    }
}
