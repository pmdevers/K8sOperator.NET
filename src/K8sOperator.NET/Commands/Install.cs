using DotMake.CommandLine;
using k8s;
using k8s.Models;
using K8sOperator.NET.Builder;
using K8sOperator.NET.Extensions;
using K8sOperator.NET.Generator;
using K8sOperator.NET.Generator.Builders;
using K8sOperator.NET.Metadata;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

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
        var watchers = dataSource.GetWatchers(serviceProvider);
        var clusterrole = CreateClusterRole(dataSource.Metadata, watchers);
        var deployment = CreateDeployment(dataSource.Metadata);

        foreach (var item in watchers)
        {
            var crd = CreateCustomResourceDefinition(item);

            Console.WriteLine(KubernetesYaml.Serialize(crd));
            Console.WriteLine("---");
        }

        Console.WriteLine(KubernetesYaml.Serialize(clusterrole));
        Console.WriteLine("---");
        Console.WriteLine(KubernetesYaml.Serialize(deployment));

        await Task.CompletedTask;
    }

    private V1CustomResourceDefinition CreateCustomResourceDefinition(IEventWatcher item)
    {
        var group = item.Metadata.OfType<KubernetesEntityAttribute>().First();

        var crdBuilder = new CustomResourceDefinitionBuilder();
        crdBuilder.WithName("testitems.sonarcloud.io")
          .WithSpec()
              .WithGroup(group.Group)
              .WithNames(
                 kind: group.Kind,
                 kindList: $"{group.Kind}List",
                 plural: group.PluralName,
                 singular: group.Kind.ToLower()
              )
              .WithScope(Scope.Namespaced)
              .WithVersion(
                    group.ApiVersion, 
                    schema=> schema.WithSchemaForType(item.Controller.ResourceType));

        return crdBuilder.Build();
    }

    private static V1Deployment CreateDeployment(IReadOnlyList<object> metadata)
    {
        var name = metadata.TryGetValue<OperatorNameMetadata, string>(x => x.Name)!;
        var image = metadata.TryGetValue<ImageMetadata, string>(x => x.GetImage())!;

        var deployment = new DeploymentBuilder();
        
        deployment.WithName($"{name}-deployment")
            .WithLabel("operator-deployment", name)
            .WithSpec()
                .WithReplicas(1)
                .WithRevisionHistory(0)
                .WithSelector(matchLabels: x => {
                    x.Add("operator-deployment", name);
                })
                .WithTemplate()
                    .WithLabel("operator-deployment", name)
                    .WithPod()
                        .WithTerminationGracePeriodSeconds(10)
                        .AddContainer()
                            .AddEnvFromObjectField("test", x => x.FieldPath = "metadata.namespace")
                            .WithName(name)
                            .WithImage(image)
                            .WithResources(
                                limits: x => {
                                    x.Add("cpu", new ResourceQuantity("100m"));
                                    x.Add("memory", new ResourceQuantity("128Mi"));
                                },
                                requests: x => {
                                    x.Add("cpu", new ResourceQuantity("100m"));
                                    x.Add("memory", new ResourceQuantity("64Mi"));
                                }
                            );

        return deployment.Build();
    }

    private static V1ClusterRole CreateClusterRole(IReadOnlyList<object> metadata, IEnumerable<IEventWatcher> watchers)
    {
        var name = metadata.TryGetValue<OperatorNameMetadata, string>(x => x.Name);

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
            .GroupBy(x => x.Group)
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

        return clusterrole.Build();
    }
}

