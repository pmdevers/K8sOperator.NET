using DotMake.CommandLine;
using k8s;
using K8sOperator.NET.Builder;
using K8sOperator.NET.Extensions;
using K8sOperator.NET.Generator;
using K8sOperator.NET.Metadata;
using System.Reflection;

namespace K8sOperator.NET.Commands;

[CliCommand(
    Name = "install",
    Description = "Install or update operator",
    Parent = typeof(Root)
    )]
internal class Install(IServiceProvider serviceProvider, IControllerDataSource dataSource)
{
    [CliOption(Description = "export")]
    public bool Export { get; set; } = true;

    [CliOption(Description = "Name of the operator")]
    public string Name { get; set; } = Assembly.GetEntryAssembly()?.GetName().Name ?? string.Empty;

    public async Task RunAsync()
    {
        var name = dataSource.Metadata.TryGetValue<OperatorNameMetadata, string>(x => x.Name);
        var image = dataSource.Metadata.TryGetValue<ImageMetadata, string>(x => x.GetImage());

        var clusterrole = ClusterRoleBuilder.Create($"{name}-role");
        var deployment = DeploymentBuilder.Create($"{name}-operator", image);

        clusterrole.AddDefaultRules();

        foreach (var item in dataSource.GetWatchers(serviceProvider))
        {
            var crd = CustomResourceGenerator.Generated(item.Controller.ResourceType, item.Metadata);
            clusterrole.AddRuleFor(item.Controller.ResourceType, item.Metadata);

            Console.WriteLine(KubernetesYaml.Serialize(crd));
            Console.WriteLine("---");
        }

        Console.WriteLine(KubernetesYaml.Serialize(clusterrole.Build()));
        Console.WriteLine("---");
        Console.WriteLine(KubernetesYaml.Serialize(deployment.Build()));

        await Task.CompletedTask;
    }
}

