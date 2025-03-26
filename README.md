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
- [Usage](#usage)
- [Configuration](#configuration)
- [Contributing](#contributing)
- [License](#license)

## Features

- Easy integration

## Installation

To install the package, use the following command in your .NET Core project:

```bash
dotnet add package K8sOperator.NET
dotnet add package K8sOperator.NET.Generators
```

Alternatively, you can add it manually to your `.csproj` file:

```xml
<PackageReference Include="K8sOperator.NET" Version="0.1.0" />
<PackageReference Include="K8sOperator.NET.Generators" Version="0.1.0" />
```

## Usage

Here are some basic examples of how to use the library:

### Setup

```csharp
using K8sOperator.NET;

var builder = OperatorHost.CreateOperatorApplicationBuilder(args);

builder.AddController<ProjectController>()
    .WithFinalizer("project.local.finalizer");

var app = builder.Build();

app.AddInstall();

await app.RunAsync();

```

### add custom launchSettings.json

```json

{
    "profiles": {
        "Operator": {
            "commandName": "Project",
            "commandLineArgs": "operator",
            "environmentVariables": {
                "ASPNETCORE_ENVIRONMENT": "Development"
            },
            "dotnetRunMessages": true
        },
        "Install": {
            "commandName": "Project",
            "commandLineArgs": "install > ./install.yaml",
            "environmentVariables": {
                "ASPNETCORE_ENVIRONMENT": "Development"
            },
            "dotnetRunMessages": true
        },
        "Help": {
            "commandName": "Project",
            "commandLineArgs": "",
            "environmentVariables": {
                "ASPNETCORE_ENVIRONMENT": "Development"
            },
            "dotnetRunMessages": true
        },
        "Version": {
            "commandName": "Project",
            "commandLineArgs": "version",
            "environmentVariables": {
                "ASPNETCORE_ENVIRONMENT": "Development"
            },
            "dotnetRunMessages": true
        }
    },
   "$schema": "http://json.schemastore.org/launchsettings.json"
}

```

## Configuration

By running the `Install` profile will create the install.yaml file in the root of the project. This file can be used to install the operator in a Kubernetes cluster.

Run the following command to install the operator:
```bash
kubectl apply -f install.yaml
```


## Contributing

Contributions are welcome! Please feel free to submit a pull request or open an issue if you encounter any bugs or have feature requests.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/fooBar`)
3. Commit your changes (`git commit -am 'Add some fooBar'`)
4. Push to the branch (`git push origin feature/fooBar`)
5. Create a new Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
