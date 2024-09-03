using k8s;
using k8s.Models;
using K8sOperator.NET.Builder;
using K8sOperator.NET.Extensions;
using K8sOperator.NET.Generator.Builders;
using K8sOperator.NET.Metadata;

namespace K8sOperator.NET.Commands;

internal class Install(IServiceProvider serviceProvider, IControllerDataSource dataSource)
{
    public async Task RunAsync()
    {
        var watchers = dataSource.GetWatchers(serviceProvider);
        var clusterrole = CreateClusterRole(dataSource.Metadata, watchers);
        var clusterrolebinding = CreateClusterRoleBinding(dataSource.Metadata);
        var deployment = CreateDeployment(dataSource.Metadata);

        foreach (var item in watchers)
        {
            var crd = CreateCustomResourceDefinition(item);
            Console.WriteLine(KubernetesYaml.Serialize(crd));
            Console.WriteLine("---");
        }
        Console.WriteLine(KubernetesYaml.Serialize(clusterrole));
        Console.WriteLine("---");
        Console.WriteLine(KubernetesYaml.Serialize(clusterrolebinding));
        Console.WriteLine("---");
        Console.WriteLine(KubernetesYaml.Serialize(deployment));

        await Task.CompletedTask;
    }

    private V1CustomResourceDefinition CreateCustomResourceDefinition(IEventWatcher item)
    {
        var group = item.Metadata.OfType<KubernetesEntityAttribute>().First();

        var crdBuilder = new CustomResourceDefinitionBuilder();
        crdBuilder.WithName($"{group.PluralName}.{group.Group}")
          .WithSpec()
              .WithGroup(group.Group)
              .WithNames(
                 kind: group.Kind,
                 kindList: $"{group.Kind}List",
                 plural: group.PluralName,
                 singular: group.Kind.ToLower()
              )
              .WithScope(EntityScope.Namespaced)
              .WithVersion(
                    group.ApiVersion, 
                    schema=> {
                        schema.WithSchemaForType(item.Controller.ResourceType);
                        schema.WithServed(true);
                        schema.WithStorage(true);
                    });

        return crdBuilder.Build();
    }

    private static V1Deployment CreateDeployment(IReadOnlyList<object> metadata)
    {
        var name = metadata.TryGetValue<IOperatorNameMetadata, string>(x => x.Name)!;
        var image = metadata.TryGetValue<DockerImageAttribute, string>(x => x.GetImage())!;

        var deployment = DeploymentBuilder.Create();
        
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
                        .WithSecurityContext(b =>
                            b.Add(x => {
                                x.RunAsNonRoot = true;
                                x.SeccompProfile = new()
                                {
                                    Type = "RuntimeDefault"
                                };
                         }))
                        .WithTerminationGracePeriodSeconds(10)
                        .AddContainer()
                            .AddEnvFromObjectField("test", x => x.FieldPath = "metadata.namespace")
                            .WithSecurityContext(x => { 
                                x.AllowPrivilegeEscalation(false);
                                x.RunAsRoot(); 
                                x.RunAsUser(2024);
                                x.RunAsGroup(2024);
                                x.WithCapabilities(x => x.WithDrop("ALL"));
                             })
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

    private V1ClusterRoleBinding CreateClusterRoleBinding(IReadOnlyList<object> metadata)
    {
        var name = metadata.TryGetValue<IOperatorNameMetadata, string>(x => x.Name);

        var clusterrolebinding = new ClusterRoleBindingBuilder()
            .WithName($"{name}-role-binding")
            .WithRoleRef("rbac.authorization.k8s.io", "ClusterRole", $"{name}-role")
            .WithSubject(kind: "ServiceAccount", name: "default", ns: "system");

        return clusterrolebinding.Build();
    }

    private static V1ClusterRole CreateClusterRole(IReadOnlyList<object> metadata, IEnumerable<IEventWatcher> watchers)
    {
        var name = metadata.TryGetValue<IOperatorNameMetadata, string>(x => x.Name);

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

