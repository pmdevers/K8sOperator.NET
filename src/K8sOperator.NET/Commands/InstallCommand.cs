using k8s;
using k8s.Models;
using K8sOperator.NET;
using K8sOperator.NET.Builder;
using K8sOperator.NET.Configuration;
using K8sOperator.NET.Generation;
using K8sOperator.NET.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace K8sOperator.NET.Commands;

[OperatorArgument("install", Description = "Generates the install manifests")]
public class InstallCommand(IHost app) : IOperatorCommand
{
    private readonly StringWriter _output = new();

    public async Task RunAsync(string[] args)
    {
        var dataSource = app.Services.GetRequiredService<EventWatcherDatasource>();
        var config = app.Services.GetRequiredService<OperatorConfiguration>();
        var watchers = dataSource.GetWatchers().ToList();
        var ns = CreateNamespace(config);
        var clusterrole = CreateClusterRole(config, watchers);
        var clusterrolebinding = CreateClusterRoleBinding(config);
        var deployment = CreateDeployment(config);

        foreach (var item in watchers)
        {
            var crd = CreateCustomResourceDefinition(item);

            await Write(crd);
        }

        await Write(clusterrole);
        await Write(clusterrolebinding);
        await Write(ns);
        await Write(deployment);

        Console.WriteLine(_output.ToString());
    }

    private async Task Write(IKubernetesObject obj)
    {
        await _output.WriteLineAsync(KubernetesYaml.Serialize(obj));
        await _output.WriteLineAsync("---");
    }

    private static V1Namespace CreateNamespace(OperatorConfiguration config)
    {
        var nsBuilder = KubernetesObjectBuilder.Create<V1Namespace>();
        nsBuilder.WithName(config.Namespace);

        return nsBuilder.Build();
    }

    private static V1CustomResourceDefinition CreateCustomResourceDefinition(IEventWatcher item)
    {
        var group = item.Metadata.OfType<KubernetesEntityAttribute>().First();
        var scope = item.Metadata.OfType<ScopeAttribute>().FirstOrDefault()
            ?? ScopeAttribute.Default;

        var columns = item.Metadata.OfType<AdditionalPrinterColumnAttribute>();

        var crdBuilder = KubernetesObjectBuilder.Create<V1CustomResourceDefinition>();
        crdBuilder
          .WithName($"{group.PluralName}.{group.Group}".ToLower())
          .WithSpec()
              .WithGroup(group.Group)
              .WithNames(
                 kind: group.Kind,
                 kindList: $"{group.Kind}List",
                 plural: group.PluralName.ToLower(),
                 singular: group.Kind.ToLower()
              )
              .WithScope(scope.Scope)
              .WithVersion(
                    group.ApiVersion,
                    schema =>
                    {
                        schema.WithSchemaForType(item.Controller.ResourceType);
                        schema.WithServed(true);
                        schema.WithStorage(true);
                        foreach (var column in columns)
                        {
                            schema.WithAdditionalPrinterColumn(column.Name, column.Type, column.Description, column.Path);
                        }

                    });


        return crdBuilder.Build();
    }

    private static V1Deployment CreateDeployment(OperatorConfiguration config)
    {
        var deployment = KubernetesObjectBuilder.Create<V1Deployment>();

        deployment
            .WithName($"{config.OperatorName}")
            .WithNamespace(config.Namespace)
            .WithLabel("operator", config.OperatorName)
            .WithSpec()
                .WithReplicas(1)
                .WithRevisionHistory(0)
                .WithSelector(matchLabels: x =>
                {
                    x.Add("operator", config.OperatorName);
                })
                .WithTemplate()
                    .WithLabel("operator", config.OperatorName)

                    .WithPod()
                        .WithSecurityContext(b =>
                            b.Add(x =>
                            {
                                x.RunAsNonRoot = true;
                                x.SeccompProfile = new()
                                {
                                    Type = "RuntimeDefault"
                                };
                            }))
                        .WithTerminationGracePeriodSeconds(10)
                        .AddContainer()
                            .AddEnvFromObjectField("NAMESPACE", x => x.FieldPath = "metadata.namespace")
                            .WithSecurityContext(x =>
                            {
                                x.AllowPrivilegeEscalation(false);
                                x.RunAsNonRoot();
                                x.RunAsUser(2024);
                                x.RunAsGroup(2024);
                                x.WithCapabilities(x => x.WithDrop("ALL"));
                            })
                            .WithName(config.OperatorName)
                            .WithImage(config.ContainerImage)
                            .WithResources(
                                limits: x =>
                                {
                                    x.Add("cpu", new ResourceQuantity("100m"));
                                    x.Add("memory", new ResourceQuantity("128Mi"));
                                },
                                requests: x =>
                                {
                                    x.Add("cpu", new ResourceQuantity("100m"));


                                    x.Add("memory", new ResourceQuantity("64Mi"));
                                }
                            );

        return deployment.Build();
    }

    private static V1ClusterRoleBinding CreateClusterRoleBinding(OperatorConfiguration config)
    {
        var clusterrolebinding = KubernetesObjectBuilder.Create<V1ClusterRoleBinding>()
            .WithName($"{config.OperatorName}-role-binding")
            .WithRoleRef("rbac.authorization.k8s.io", "ClusterRole", $"{config.OperatorName}-role")
            .WithSubject(kind: "ServiceAccount", name: "default", ns: config.Namespace);

        return clusterrolebinding.Build();
    }

    private static V1ClusterRole CreateClusterRole(OperatorConfiguration config, IEnumerable<IEventWatcher> watchers)
    {
        var clusterrole = KubernetesObjectBuilder.Create<V1ClusterRole>()
                    .WithName($"{config.OperatorName}-role");

        clusterrole.AddRule()
            .WithGroups("")
            .WithResources("events")
            .WithVerbs("get", "list", "create", "update");

        clusterrole.AddRule()
            .WithGroups("coordination.k8s.io")
            .WithResources("leases")
            .WithVerbs("create", "update", "get");

        var rules = watchers
            .Select(x => x.Metadata.OfType<KubernetesEntityAttribute>().First())
            .GroupBy(x => x.Group)
            .ToList();

        foreach (var item in rules)
        {
            clusterrole.AddRule()
                    .WithGroups(item.Key)
                    .WithResources([.. item.Select(x => x.PluralName)])
                    .WithVerbs("*");
            clusterrole.AddRule()
                    .WithGroups(item.Key)
                    .WithResources([.. item.Select(x => $"{x.PluralName}/status")])
                    .WithVerbs("get", "update", "patch");
        }

        return clusterrole.Build();
    }

}
