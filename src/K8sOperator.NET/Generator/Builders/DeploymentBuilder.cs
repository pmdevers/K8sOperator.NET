using k8s;
using k8s.Models;

namespace K8sOperator.NET.Generator.Builders;

public class DeploymentBuilder : KubernetesObjectBuilderWithMetaData<V1Deployment> { }

public static class DeploymentBuilderExtensions
{
    public static IKubernetesObjectBuilder<V1DeploymentSpec> WithSpec<TBuilder>(this TBuilder builder)
        where TBuilder: IKubernetesObjectBuilder<V1Deployment>
    {
        var specBuilder = new KubernetesObjectBuilder<V1DeploymentSpec>();
        builder.Add(x => x.Spec = specBuilder.Build());
        return specBuilder;
    }

    public static TBuilder WithReplicas<TBuilder>(this TBuilder builder, int replicas = 1)
        where TBuilder : IKubernetesObjectBuilder<V1DeploymentSpec>
    {
        builder.Add(x => x.Replicas = replicas);
        return builder;
    }
    public static TBuilder WitRevisionHistory<TBuilder>(this TBuilder builder, int revisionHistoryLimit = 0)
        where TBuilder : IKubernetesObjectBuilder<V1DeploymentSpec>
    {
        builder.Add(x => x.RevisionHistoryLimit = revisionHistoryLimit);
        return builder;
    }

    public static IKubernetesObjectBuilder<V1PodTemplateSpec> WithTemplate<TBuilder>(this TBuilder builder)
        where TBuilder : IKubernetesObjectBuilder<V1DeploymentSpec>
    {
        var podBuilder = new KubernetesObjectBuilder<V1PodTemplateSpec>();
        builder.Add(x => x.Template = podBuilder.Build());
        return podBuilder;
    }

    public static IKubernetesObjectBuilder<V1PodSpec> WithPod<TBuilder>(this TBuilder builder)
        where TBuilder : IKubernetesObjectBuilder<V1PodTemplateSpec>
    {
        var podBuilder = new KubernetesObjectBuilder<V1PodSpec>();
        builder.Add(x => x.Spec = podBuilder.Build());
        return podBuilder;
    }

    public static IKubernetesObjectBuilder<V1Container> AddContainer<TBuilder>(this TBuilder builder)
        where TBuilder : IKubernetesObjectBuilder<V1PodSpec>
    {
        var b = new ContainerBuilder();
        builder.Add(x =>
        {
            x.Containers ??= [];
            x.Containers.Add(b.Build());
        });
        return b;
    }
}

internal class ContainerBuilder : KubernetesObjectBuilder<V1Container>
{
    
}
