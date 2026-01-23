using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace K8sOperator.NET.BuildTasks;

public class GenerateDockerfileTask : Task
{
    [Required]
    public string ProjectDirectory { get; set; } = string.Empty;

    [Required]
    public string ProjectName { get; set; } = string.Empty;

    [Required]
    public string TargetFramework { get; set; } = string.Empty;

    [Required]
    public string OperatorName { get; set; } = string.Empty;

    [Required]
    public string ContainerRegistry { get; set; } = string.Empty;

    [Required]
    public string ContainerRepository { get; set; } = string.Empty;

    [Required]
    public string ContainerTag { get; set; } = string.Empty;

    public override bool Execute()
    {
        try
        {
            var dockerfilePath = Path.Combine(ProjectDirectory, "Dockerfile");
            var dockerignorePath = Path.Combine(ProjectDirectory, ".dockerignore");

            // Extract .NET version from TargetFramework
            var dotnetVersion = ExtractDotNetVersion(TargetFramework);

            // Read templates from embedded resources
            var dockerfileContent = ReadEmbeddedResource("Dockerfile.template");
            var dockerignoreContent = ReadEmbeddedResource(".dockerignore.template");

            // Replace placeholders
            dockerfileContent = dockerfileContent
                .Replace("{PROJECT_NAME}", ProjectName)
                .Replace("{DOTNET_VERSION}", dotnetVersion);

            if (!File.Exists(dockerfilePath))
            {
                File.WriteAllText(dockerfilePath, dockerfileContent);

                Log.LogMessage(MessageImportance.High, $"Generated Dockerfile at: {dockerfilePath}");

            }

            if (!File.Exists(dockerignorePath))
            {
                File.WriteAllText(dockerignorePath, dockerignoreContent);

                Log.LogMessage(MessageImportance.High, $"Generated .dockerignore at: {dockerignorePath}");
            }

            // Log success
            Log.LogMessage(MessageImportance.High, $"Operator: {OperatorName}");
            Log.LogMessage(MessageImportance.High, $"  .NET Version: {dotnetVersion}");
            Log.LogMessage(MessageImportance.High, $"   Image: {ContainerRegistry}/{ContainerRepository}:{ContainerTag}");
            Log.LogMessage(MessageImportance.High, "");
            Log.LogMessage(MessageImportance.High, "To build the image:");
            Log.LogMessage(MessageImportance.High, $"  docker build -t {ContainerRegistry}/{ContainerRepository}:{ContainerTag} .");
            Log.LogMessage(MessageImportance.High, "");
            Log.LogMessage(MessageImportance.High, "To push the image:");
            Log.LogMessage(MessageImportance.High, $"  docker push {ContainerRegistry}/{ContainerRepository}:{ContainerTag}");
            Log.LogMessage(MessageImportance.High, "");

            return true;
        }
        catch (Exception ex)
        {
            Log.LogError($"Failed to generate Dockerfile: {ex.Message}");
            return false;
        }
    }

    private static string ExtractDotNetVersion(string targetFramework)
    {
        // Handle formats like "net10.0", "net8.0", "netcoreapp3.1"
        var match = Regex.Match(targetFramework, @"net(?:coreapp)?(\d+\.\d+)");
        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        // Fallback
        return "10.0";
    }

    private static string ReadEmbeddedResource(string resourceName)
    {
        var assembly = typeof(GenerateDockerfileTask).Assembly;
        var fullResourceName = $"K8sOperator.NET.BuildTasks.Templates.{resourceName}";

        using var stream = assembly.GetManifestResourceStream(fullResourceName)
            ?? throw new InvalidOperationException($"Could not find embedded resource: {fullResourceName}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
