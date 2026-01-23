using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.IO;

namespace K8sOperator.NET.BuildTasks;

public class GenerateLaunchSettingsTask : Task
{
    [Required]
    public string ProjectDirectory { get; set; } = string.Empty;

    public override bool Execute()
    {
        try
        {
            var propertiesDir = Path.Combine(ProjectDirectory, "Properties");
            var launchSettingsPath = Path.Combine(propertiesDir, "launchSettings.json");

            if (File.Exists(launchSettingsPath))
            {
                Log.LogMessage(MessageImportance.Normal, $"launchSettings.json already exists at {launchSettingsPath}, skipping generation");
                return true;
            }

            // Read template from embedded resources
            var launchSettingsContent = ReadEmbeddedResource("launchSettings.json.template");

            // Create Properties directory if needed
            Directory.CreateDirectory(propertiesDir);

            // Write launchSettings.json
            File.WriteAllText(launchSettingsPath, launchSettingsContent);

            // Log success
            Log.LogMessage(MessageImportance.High, "");
            Log.LogMessage(MessageImportance.High, $"Generated launchSettings.json at: {launchSettingsPath}");
            Log.LogMessage(MessageImportance.High, $"  Profiles: Operator, Install, Version, Help");
            Log.LogMessage(MessageImportance.High, "");

            return true;
        }
        catch (Exception ex)
        {
            Log.LogError($"Failed to generate launchSettings.json: {ex.Message}");
            return false;
        }
    }

    private static string ReadEmbeddedResource(string resourceName)
    {
        var assembly = typeof(GenerateLaunchSettingsTask).Assembly;
        var fullResourceName = $"K8sOperator.NET.BuildTasks.Templates.{resourceName}";

        using var stream = assembly.GetManifestResourceStream(fullResourceName)
            ?? throw new InvalidOperationException($"Could not find embedded resource: {fullResourceName}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}


