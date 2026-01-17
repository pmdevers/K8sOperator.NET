using K8sOperator.NET.Builder;
using K8sOperator.NET.Metadata;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using System.Text;

namespace K8sOperator.NET.Commands;

[OperatorArgument("generate-dockerfile", Description = "Generates a Dockerfile for the operator", Order = 101, ShowInHelp = false)]
public class GenerateDockerfileCommand(IHost host) : IOperatorCommand
{
    public async Task RunAsync(string[] args)
    {
        var assembly = Assembly.GetEntryAssembly();
        var operatorName = assembly?.GetCustomAttribute<OperatorNameAttribute>()?.OperatorName
            ?? OperatorNameAttribute.Default.OperatorName;
        var dockerImage = assembly?.GetCustomAttribute<DockerImageAttribute>()
            ?? DockerImageAttribute.Default;

        var projectName = AppDomain.CurrentDomain.FriendlyName.Replace(".dll", "");
        var targetFramework = "net10.0"; // Can be made dynamic if needed

        var dockerfile = GenerateDockerfileContent(projectName, targetFramework, operatorName);

        var dockerfilePath = Path.Combine(Directory.GetCurrentDirectory(), "Dockerfile");
        await File.WriteAllTextAsync(dockerfilePath, dockerfile);

        var dockerignorePath = Path.Combine(Directory.GetCurrentDirectory(), ".dockerignore");
        await File.WriteAllTextAsync(dockerignorePath, GenerateDockerignoreContent());

        Console.WriteLine($"✅ Generated Dockerfile at: {dockerfilePath}");
        Console.WriteLine($"✅ Generated .dockerignore at: {dockerignorePath}");
        Console.WriteLine($"   Operator: {operatorName}");
        Console.WriteLine($"   Image: {dockerImage.Registry}/{dockerImage.Repository}:{dockerImage.Tag}");
        Console.WriteLine();
        Console.WriteLine("To build the image:");
        Console.WriteLine($"  docker build -t {dockerImage.Registry}/{dockerImage.Repository}:{dockerImage.Tag} .");
        Console.WriteLine();
        Console.WriteLine("To push the image:");
        Console.WriteLine($"  docker push {dockerImage.Registry}/{dockerImage.Repository}:{dockerImage.Tag}");
    }

    private static string GenerateDockerfileContent(string projectName, string targetFramework, string operatorName)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# Build stage");
        sb.AppendLine("FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build");
        sb.AppendLine("WORKDIR /src");
        sb.AppendLine();
        sb.AppendLine("# Copy project file and restore dependencies");
        sb.AppendLine($"COPY [\"{projectName}.csproj\", \"./\"]");
        sb.AppendLine("RUN dotnet restore");
        sb.AppendLine();
        sb.AppendLine("# Copy source code and build");
        sb.AppendLine("COPY . .");
        sb.AppendLine("RUN dotnet build -c Release -o /app/build");
        sb.AppendLine();
        sb.AppendLine("# Publish stage");
        sb.AppendLine("FROM build AS publish");
        sb.AppendLine("RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false");
        sb.AppendLine();
        sb.AppendLine("# Runtime stage");
        sb.AppendLine("FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final");
        sb.AppendLine("WORKDIR /app");
        sb.AppendLine();
        sb.AppendLine("# Create non-root user");
        sb.AppendLine("RUN groupadd -r operator && useradd -r -g operator operator");
        sb.AppendLine();
        sb.AppendLine("# Copy published app");
        sb.AppendLine("COPY --from=publish /app/publish .");
        sb.AppendLine();
        sb.AppendLine("# Set ownership");
        sb.AppendLine("RUN chown -R operator:operator /app");
        sb.AppendLine();
        sb.AppendLine("# Switch to non-root user");
        sb.AppendLine("USER operator");
        sb.AppendLine();
        sb.AppendLine("# Set environment variables");
        sb.AppendLine("ENV ASPNETCORE_ENVIRONMENT=Production \\");
        sb.AppendLine("    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \\");
        sb.AppendLine("    DOTNET_EnableDiagnostics=0");
        sb.AppendLine();
        sb.AppendLine("# Health check");
        sb.AppendLine("HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \\");
        sb.AppendLine($"  CMD dotnet {projectName}.dll version || exit 1");
        sb.AppendLine();
        sb.AppendLine("# Entrypoint");
        sb.AppendLine($"ENTRYPOINT [\"dotnet\", \"{projectName}.dll\"]");
        sb.AppendLine("CMD [\"operator\"]");

        return sb.ToString();
    }

    private static string GenerateDockerignoreContent()
    {
        var sb = new StringBuilder();

        sb.AppendLine("# Git");
        sb.AppendLine(".git");
        sb.AppendLine(".gitignore");
        sb.AppendLine(".gitattributes");
        sb.AppendLine();
        sb.AppendLine("# Build results");
        sb.AppendLine("bin/");
        sb.AppendLine("obj/");
        sb.AppendLine("[Bb]uild/");
        sb.AppendLine("[Dd]ebug/");
        sb.AppendLine("[Rr]elease/");
        sb.AppendLine();
        sb.AppendLine("# Visual Studio");
        sb.AppendLine(".vs/");
        sb.AppendLine(".vscode/");
        sb.AppendLine("*.user");
        sb.AppendLine("*.suo");
        sb.AppendLine("*.userosscache");
        sb.AppendLine("*.sln.docstates");
        sb.AppendLine();
        sb.AppendLine("# Test results");
        sb.AppendLine("[Tt]est[Rr]esult*/");
        sb.AppendLine("[Bb]uild[Ll]og.*");
        sb.AppendLine("TestResults/");
        sb.AppendLine();
        sb.AppendLine("# NuGet");
        sb.AppendLine("*.nupkg");
        sb.AppendLine("*.snupkg");
        sb.AppendLine("packages/");
        sb.AppendLine();
        sb.AppendLine("# Docker");
        sb.AppendLine("Dockerfile");
        sb.AppendLine(".dockerignore");
        sb.AppendLine();
        sb.AppendLine("# Kubernetes");
        sb.AppendLine("*.yaml");
        sb.AppendLine("*.yml");
        sb.AppendLine();
        sb.AppendLine("# Documentation");
        sb.AppendLine("*.md");
        sb.AppendLine("README*");
        sb.AppendLine("LICENSE");
        sb.AppendLine();
        sb.AppendLine("# IDE");
        sb.AppendLine(".idea/");
        sb.AppendLine("*.swp");
        sb.AppendLine("*.swo");
        sb.AppendLine("*~");

        return sb.ToString();
    }
}
