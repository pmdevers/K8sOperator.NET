# K8sOperator.NET

![Github Release](https://img.shields.io/github/v/release/pmdevers/K8sOperator.NET) 
![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/pmdevers/K8sOperator.NET/.github%2Fworkflows%2Fbuild-publish.yml) 
![GitHub License](https://img.shields.io/github/license/pmdevers/K8sOperator.NET) 
![Github Issues Open](https://img.shields.io/github/issues/pmdevers/K8sOperator.NET) 
![Github Pull Request Open](https://img.shields.io/github/issues-pr/pmdevers/K8sOperator.NET) 
[![Scheduled Code Security Testing](https://github.com/pmdevers/K8sOperator.NET/actions/workflows/security-analysis.yml/badge.svg?event=schedule)](https://github.com/pmdevers/K8sOperator.NET/actions/workflows/security-analysis.yml)

K8sOperator.NET is a powerful and intuitive library designed for creating Kubernetes Operators using C#. It simplifies the development of robust, cloud-native operators by leveraging the full capabilities of the .NET ecosystem, making it easier than ever to manage complex Kubernetes workloads with custom automation.

![Alt text](https://raw.githubusercontent.com/pmdevers/K8sOperator.NET/master/assets/logo.png "logo")

## Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Usage](#usage)
  - [Creating a Custom Resource](#creating-a-custom-resource)
  - [Implementing a Controller](#implementing-a-controller)
  - [Setting Up the Operator](#setting-up-the-operator)
- [Commands](#commands)
- [Configuration](#configuration)
  - [MSBuild Properties](#msbuild-properties)
  - [Auto-Generated Files](#auto-generated-files)
- [Docker Support](#docker-support)
- [Contributing](#contributing)
- [License](#license)

## Features

- 🚀 **Easy Integration** - Simple, intuitive API for building Kubernetes operators
- 🎯 **Custom Resource Support** - Built-in support for Custom Resource Definitions (CRDs)
- 🔄 **Automatic Reconciliation** - Event-driven reconciliation with finalizer support
- 📦 **MSBuild Integration** - Automatic generation of manifests, Docker files, and launch settings
- 🐳 **Docker Ready** - Generate optimized Dockerfiles with best practices
- 🛠️ **Built-in Commands** - Help, version, install, and code generation commands
- 🔐 **Security First** - Non-root containers, RBAC support, and security best practices
- 📝 **Source Generators** - Compile-time generation of boilerplate code
- 🎨 **Flexible Configuration** - MSBuild properties for operator customization
- 🧪 **Testable** - Built with testing in mind

## Installation

To install K8sOperator.NET, add the package to your .NET project:

```bash
dotnet add package K8sOperator.NET
```

Or add it manually to your `.csproj` file:

```xml
<PackageReference Include="K8sOperator.NET" Version="*" />
```

## Quick Start

Create a new ASP.NET Core Web Application and add K8sOperator.NET:

```bash
dotnet new web -n MyOperator
cd MyOperator
dotnet add package K8sOperator.NET
```

Update your `Program.cs`:

```csharp
using K8sOperator.NET;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOperator();

var app = builder.Build();

// Map your controllers here
// app.MapController<MyController>();

await app.RunOperatorAsync();
```

## Usage

### Creating a Custom Resource

Define your custom resource by inheriting from `CustomResource`:

```csharp
using K8sOperator.NET;

[KubernetesEntity(
    Group = "example.com",
    ApiVersion = "v1",
    Kind = "MyResource",
    PluralName = "myresources")]
public class MyResource : CustomResource<MyResource.MySpec, MyResource.MyStatus>
{
    public class MySpec
    {
        public string Name { get; set; } = string.Empty;
        public int Replicas { get; set; } = 1;
    }

    public class MyStatus
    {
        public string Phase { get; set; } = "Pending";
        public DateTime? LastUpdated { get; set; }
    }
}
```

### Implementing a Controller

Create a controller to handle your custom resource:

```csharp
using K8sOperator.NET;
using Microsoft.Extensions.Logging;

public class MyController : OperatorController<MyResource>
{
    private readonly ILogger<MyController> _logger;

    public MyController(ILogger<MyController> logger)
    {
        _logger = logger;
    }

    public override async Task AddOrModifyAsync(
        MyResource resource, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Reconciling {Name} with {Replicas} replicas",
            resource.Spec.Name,
            resource.Spec.Replicas);

        // Your reconciliation logic here
        resource.Status = new MyResource.MyStatus
        {
            Phase = "Running",
            LastUpdated = DateTime.UtcNow
        };

        await Task.CompletedTask;
    }

    public override async Task DeleteAsync(
        MyResource resource, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting {Name}", resource.Metadata.Name);
        
        // Cleanup logic here
        await Task.CompletedTask;
    }

    public override async Task FinalizeAsync(
        MyResource resource, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Finalizing {Name}", resource.Metadata.Name);
        
        // Finalization logic here
        await Task.CompletedTask;
    }
}
```

### Setting Up the Operator

Wire everything together in `Program.cs`:

```csharp
using K8sOperator.NET;
using MyOperator;

var builder = WebApplication.CreateBuilder(args);

// Add operator services
builder.Services.AddOperator();

var app = builder.Build();

// Map the controller to watch MyResource
app.MapController<MyController>();

// Run the operator
await app.RunOperatorAsync();
```

## Commands

K8sOperator.NET includes several built-in commands:

| Command | Description | Availability |
|---------|-------------|--------------|
| `operator` | Run the operator (watches for resources) | All builds |
| `install` | Generate Kubernetes installation manifests | All builds |
| `version` | Display version information | All builds |
| `help` | Show available commands | All builds |
| `generate-launchsettings` | Generate Visual Studio launch profiles | Debug only |
| `generate-dockerfile` | Generate optimized Dockerfile | Debug only |

**Note:** The `generate-*` commands are development tools and are only available in Debug builds. They are automatically excluded from Release builds to keep your production operator lean.

### Running Commands

```bash
# Run the operator
dotnet run -- operator

# Generate installation manifests
dotnet run -- install > install.yaml

# Show version
dotnet run -- version

# Generate launch settings (Debug only)
dotnet run -c Debug -- generate-launchsettings

# Generate Dockerfile (Debug only)
dotnet run -c Debug -- generate-dockerfile
```

## Configuration

### MSBuild Properties

K8sOperator.NET uses MSBuild properties to configure your operator. Add these to your `.csproj` file:

```xml
<PropertyGroup>
  <!-- Operator Configuration -->
  <OperatorName>my-operator</OperatorName>
  <OperatorNamespace>my-namespace</OperatorNamespace>
  
  <!-- Container Configuration -->
  <ContainerRegistry>ghcr.io</ContainerRegistry>
  <ContainerRepository>myorg/my-operator</ContainerRepository>
  <ContainerImageTag>1.0.0</ContainerImageTag>
  <ContainerFamily>alpine</ContainerFamily>
  
  <!-- Auto-generation (opt-in) -->
  <CreateOperatorLaunchSettings>true</CreateOperatorLaunchSettings>
  <GenerateOperatorDockerfile>true</GenerateOperatorDockerfile>
</PropertyGroup>
```

#### Available Properties

| Property | Default | Description |
|----------|---------|-------------|
| `OperatorName` | `{project-name}` | Name of the operator |
| `OperatorNamespace` | `{project-name}-system` | Kubernetes namespace |
| `ContainerRegistry` | `ghcr.io` | Container registry URL |
| `ContainerRepository` | `{Company}/{OperatorName}` | Repository path |
| `ContainerImageTag` | `{Version}` | Image tag |
| `ContainerFamily` | (empty) | Image variant (e.g., `alpine`, `distroless`) |
| `CreateOperatorLaunchSettings` | `false` | Auto-generate launch profiles |
| `GenerateOperatorDockerfile` | `false` | Auto-generate Dockerfile |

### Auto-Generated Files

When enabled, K8sOperator.NET automatically generates:

#### 1. Assembly Attributes

Metadata is embedded in your assembly:

```csharp
[assembly: OperatorNameAttribute("my-operator")]
[assembly: NamespaceAttribute("my-namespace")]
[assembly: DockerImageAttribute("ghcr.io", "myorg/my-operator", "1.0.0-alpine")]
```

#### 2. Launch Settings (`Properties/launchSettings.json`)

Visual Studio launch profiles for all registered commands:

```json
{
  "profiles": {
    "Operator": {
      "commandName": "Project",
      "commandLineArgs": "operator",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "Install": {
      "commandName": "Project",
      "commandLineArgs": "install > ./install.yaml"
    }
  }
}
```

#### 3. Dockerfile

Optimized multi-stage Dockerfile with security best practices:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["MyOperator.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN groupadd -r operator && useradd -r -g operator operator
COPY --from=build /app/publish .
RUN chown -R operator:operator /app
USER operator
ENTRYPOINT ["dotnet", "MyOperator.dll"]
CMD ["operator"]
```

#### 4. .dockerignore

Optimized Docker ignore file to reduce image size.

## Docker Support

### Building Docker Images

K8sOperator.NET generates production-ready Dockerfiles with:

- ✅ Multi-stage builds for smaller images
- ✅ Non-root user for security
- ✅ Health checks
- ✅ .NET 10 runtime
- ✅ Optimized layer caching

Generate a Dockerfile:

```bash
dotnet run -- generate-dockerfile
```

Build the image:

```bash
docker build -t ghcr.io/myorg/my-operator:1.0.0 .
```

Push to registry:

```bash
docker push ghcr.io/myorg/my-operator:1.0.0
```

### Installing in Kubernetes

Generate installation manifests:

```bash
dotnet run -- install > install.yaml
```

The generated manifest includes:
- Custom Resource Definitions (CRDs)
- ServiceAccount
- ClusterRole and ClusterRoleBinding
- Deployment

Apply to your cluster:

```bash
kubectl apply -f install.yaml
```

### Verify Installation

```bash
# Check if operator is running
kubectl get pods -n my-namespace

# View operator logs
kubectl logs -n my-namespace deployment/my-operator -f

# Check CRDs
kubectl get crds

# Create a custom resource
kubectl apply -f my-resource.yaml
```



## Contributing

Contributions are welcome! Please feel free to submit a pull request or open an issue if you encounter any bugs or have feature requests.

### Development Setup

1. Clone the repository
   ```bash
   git clone https://github.com/pmdevers/K8sOperator.NET.git
   cd K8sOperator.NET
   ```

2. Build the solution
   ```bash
   dotnet build
   ```

3. Run tests
   ```bash
   dotnet test
   ```

4. Run the example operator
   ```bash
   cd examples/SimpleOperator
   dotnet run -- operator
   ```

### Contribution Guidelines

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

Please ensure:
- ✅ All tests pass
- ✅ Code follows existing style conventions
- ✅ New features include tests
- ✅ Documentation is updated

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE.md) file for details.

---

**Built with ❤️ using .NET 10**

For more examples and documentation, visit the [GitHub repository](https://github.com/pmdevers/K8sOperator.NET).
