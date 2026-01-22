using K8sOperator.NET.Builder;
using K8sOperator.NET.Metadata;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using System.Text.RegularExpressions;

namespace K8sOperator.NET.Commands;

[OperatorArgument("generate-dockerfile", Description = "Generates a Dockerfile for the operator", Order = 101)]
public partial class GenerateDockerfileCommand(IHost host) : IOperatorCommand
{
    private readonly IHost _host = host;

    [GeneratedRegex(@"Version=v?(\d+\.\d+)", RegexOptions.Compiled)]
    private static partial Regex VersionRegex();

    public async Task RunAsync(string[] args)
    {
        var assembly = Assembly.GetEntryAssembly();
        var operatorName = assembly?.GetCustomAttribute<OperatorNameAttribute>()?.OperatorName
            ?? OperatorNameAttribute.Default.OperatorName;
        var dockerImage = assembly?.GetCustomAttribute<DockerImageAttribute>()
            ?? DockerImageAttribute.Default;

        var projectName = AppDomain.CurrentDomain.FriendlyName.Replace(".dll", "");

        // Get the .NET version from the assembly's target framework
        var dotnetVersion = GetDotNetVersion(assembly);

        // Read templates from embedded resources
        var dockerfileContent = await ReadEmbeddedResourceAsync("Dockerfile.template");
        var dockerignoreContent = await ReadEmbeddedResourceAsync(".dockerignore.template");

        // Replace placeholders
        dockerfileContent = dockerfileContent
            .Replace("{PROJECT_NAME}", projectName)
            .Replace("{DOTNET_VERSION}", dotnetVersion);

        var dockerfilePath = Path.Combine(Directory.GetCurrentDirectory(), "Dockerfile");
        await File.WriteAllTextAsync(dockerfilePath, dockerfileContent);

        var dockerignorePath = Path.Combine(Directory.GetCurrentDirectory(), ".dockerignore");
        await File.WriteAllTextAsync(dockerignorePath, dockerignoreContent);

        Console.WriteLine($"? Generated Dockerfile at: {dockerfilePath}");
        Console.WriteLine($"? Generated .dockerignore at: {dockerignorePath}");
        Console.WriteLine($"   Operator: {operatorName}");
        Console.WriteLine($"   .NET Version: {dotnetVersion}");
        Console.WriteLine($"   Image: {dockerImage.Registry}/{dockerImage.Repository}:{dockerImage.Tag}");
        Console.WriteLine();
        Console.WriteLine("To build the image:");
        Console.WriteLine($"  docker build -t {dockerImage.Registry}/{dockerImage.Repository}:{dockerImage.Tag} .");
        Console.WriteLine();
        Console.WriteLine("To push the image:");
        Console.WriteLine($"  docker push {dockerImage.Registry}/{dockerImage.Repository}:{dockerImage.Tag}");
    }

    private static string GetDotNetVersion(Assembly? assembly)
    {
        if (assembly == null)
            return "10.0"; // Default fallback

        // Get the target framework attribute
        var targetFrameworkAttr = assembly.GetCustomAttribute<System.Runtime.Versioning.TargetFrameworkAttribute>();

        if (targetFrameworkAttr == null)
            return "10.0"; // Default fallback

        // Extract version from framework name (e.g., ".NETCoreApp,Version=v10.0" -> "10.0")
        var frameworkName = targetFrameworkAttr.FrameworkName;

        // Try to parse the version using the generated regex
        var versionMatch = VersionRegex().Match(frameworkName);

        if (versionMatch.Success)
        {
            return versionMatch.Groups[1].Value;
        }

        return "10.0"; // Default fallback
    }

    private static async Task<string> ReadEmbeddedResourceAsync(string resourceName)
    {
        var assembly = typeof(GenerateDockerfileCommand).Assembly;
        var fullResourceName = $"K8sOperator.NET.Templates.{resourceName}";

        await using var stream = assembly.GetManifestResourceStream(fullResourceName)
            ?? throw new InvalidOperationException($"Could not find embedded resource: {fullResourceName}");
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}
