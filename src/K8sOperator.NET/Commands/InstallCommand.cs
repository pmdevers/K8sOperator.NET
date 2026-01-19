using k8s;
using k8s.Models;
using K8sOperator.NET;
using K8sOperator.NET.Builder;
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
        var watchers = dataSource.GetWatchers().ToList();
        var ns = CreateNamespace(dataSource.Metadata);
        var clusterrole = CreateClusterRole(dataSource.Metadata, watchers);
        var clusterrolebinding = CreateClusterRoleBinding(dataSource.Metadata);
        var deployment = CreateDeployment(dataSource.Metadata);

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

    private static V1Namespace CreateNamespace(IReadOnlyList<object> metadata)
    {
        var ns = metadata.OfType<NamespaceAttribute>().FirstOrDefault()
            ?? NamespaceAttribute.Default;

        var nsBuilder = KubernetesObjectBuilder.Create<V1Namespace>();
        nsBuilder.WithName(ns.Namespace);

        return nsBuilder.Build();
    }

    private static V1CustomResourceDefinition CreateCustomResourceDefinition(IEventWatcher item)
    {
        var group = item.Metadata.OfType<KubernetesEntityAttribute>().First();
        var scope = item.Metadata.OfType<ScopeAttribute>().FirstOrDefault()
            ?? ScopeAttribute.Default;

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
                    });

        return crdBuilder.Build();
    }

    private static V1Deployment CreateDeployment(IReadOnlyList<object> metadata)
    {
        var name = metadata.OfType<OperatorNameAttribute>().FirstOrDefault()
            ?? OperatorNameAttribute.Default;
        var image = metadata.OfType<DockerImageAttribute>().FirstOrDefault()
            ?? DockerImageAttribute.Default;
        var ns = metadata.OfType<NamespaceAttribute>().FirstOrDefault()
            ?? NamespaceAttribute.Default;

        var deployment = KubernetesObjectBuilder.Create<V1Deployment>();

        deployment
            .WithName($"{name.OperatorName}")
            .WithNamespace(ns.Namespace)
            .WithLabel("operator", name.OperatorName)
            .WithSpec()
                .WithReplicas(1)
                .WithRevisionHistory(0)
                .WithSelector(matchLabels: x =>
                {
                    x.Add("operator", name.OperatorName);
                })
                .WithTemplate()
                    .WithLabel("operator", name.OperatorName)

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
                                x.RunAsRoot();
                                x.RunAsUser(2024);
                                x.RunAsGroup(2024);
                                x.WithCapabilities(x => x.WithDrop("ALL"));
                            })
                            .WithName(name.OperatorName)
                            .WithImage(image.GetImage())
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

    private static V1ClusterRoleBinding CreateClusterRoleBinding(IReadOnlyList<object> metadata)
    {
        var name = metadata.OfType<OperatorNameAttribute>().FirstOrDefault()
            ?? OperatorNameAttribute.Default;

        var clusterrolebinding = KubernetesObjectBuilder.Create<V1ClusterRoleBinding>()
            .WithName($"{name.OperatorName}-role-binding")
            .WithRoleRef("rbac.authorization.k8s.io", "ClusterRole", $"{name.OperatorName}-role")
            .WithSubject(kind: "ServiceAccount", name: "default", ns: "system");

        return clusterrolebinding.Build();
    }

    private static V1ClusterRole CreateClusterRole(IReadOnlyList<object> metadata, IEnumerable<IEventWatcher> watchers)
    {
        var name = metadata.OfType<OperatorNameAttribute>().FirstOrDefault()
            ?? OperatorNameAttribute.Default;

        var clusterrole = KubernetesObjectBuilder.Create<V1ClusterRole>()
                    .WithName($"{name.OperatorName}-role");

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
