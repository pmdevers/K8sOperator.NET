using DotMake.CommandLine;
using k8s;
using k8s.Models;
using K8sOperator.NET.Builder;
using K8sOperator.NET.Extensions;
using K8sOperator.NET.Generator;
using K8sOperator.NET.Generator.Builders;
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

        var watchers = dataSource.GetWatchers(serviceProvider);

        var clusterrole = new ClusterRoleBuilder()
            .WithName($"{name}-role");

        clusterrole.AddRule()
            .WithGroups("")
            .WithResources("events")
            .WithVerbs("get", "list", "create", "update");

        clusterrole.AddRule()
            .WithGroups("coordination.k8s.io")
            .WithResources("leases")
            .WithVerbs("*");

        var rules = watchers
            .Select(x => x.Metadata.OfType<KubernetesEntityAttribute>().First())
            .GroupBy(x=>x.Group)
            .ToList();

        foreach (var item in rules)
        {
            clusterrole.AddRule()
                    .WithGroups(item.Key)
                    .WithResources(item.Select(x => x.PluralName).ToArray())
                    .WithVerbs("*");
            clusterrole.AddRule()
                    .WithGroups(item.Key)
                    .WithResources(item.Select(x => $"{x.PluralName}/status").ToArray())
                    .WithVerbs("get", "update", "patch");
        }
        
        var deployment = new DeploymentBuilder(); //DeploymentBuilder.Create($"{name}-operator", image);

        deployment.WithName(name);
        deployment.WithSpec()
                .WithTemplate()
                .WithPod()
                .AddContainer()
                .Add(x => x.Image = image);

        foreach (var item in watchers)
        {
            var crd = CustomResourceGenerator.Generated(item.Controller.ResourceType, item.Metadata);

            Console.WriteLine(KubernetesYaml.Serialize(crd));
            Console.WriteLine("---");
        }

        Console.WriteLine(KubernetesYaml.Serialize(clusterrole.Build()));
        Console.WriteLine("---");
        Console.WriteLine(KubernetesYaml.Serialize(deployment.Build()));

        await Task.CompletedTask;
    }
}

