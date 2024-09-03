using k8s.Authentication;
using k8s.Models;

namespace K8sOperator.NET.Generator.Builders;

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
    public static TBuilder WithRevisionHistory<TBuilder>(this TBuilder builder, int revisionHistoryLimit = 0)
        where TBuilder : IKubernetesObjectBuilder<V1DeploymentSpec>
    {
        builder.Add(x => x.RevisionHistoryLimit = revisionHistoryLimit);
        return builder;
    }

    public static TBuilder WithSelector<TBuilder>(this TBuilder builder, 
        Action<IList<V1LabelSelectorRequirement>> matchExpressions = null,
        Action<Dictionary<string, string>> matchLabels = null)
        where TBuilder : IKubernetesObjectBuilder<V1DeploymentSpec>
    {
        var labels = new Dictionary<string, string>();
        matchLabels?.Invoke(labels);

        var expressions = new List<V1LabelSelectorRequirement>();
        matchExpressions?.Invoke(expressions);

        builder.Add(x => x.Selector = new() { 
            MatchLabels = matchLabels is null ? null : labels,
            MatchExpressions = matchExpressions is null ? null : expressions
        }); 
        return builder;
    }

    public static IKubernetesObjectBuilder<V1PodTemplateSpec> WithTemplate<TBuilder>(this TBuilder builder)
        where TBuilder : IKubernetesObjectBuilder<V1DeploymentSpec>
    {
        var podBuilder = new KubernetesObjectBuilderWithMetaData<V1PodTemplateSpec>();
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

    public static TBuilder WithSecurityContext<TBuilder>(this TBuilder builder, Action<IKubernetesObjectBuilder<V1PodSecurityContext>> securityContext)
        where TBuilder : IKubernetesObjectBuilder<V1PodSpec>
    {
        var b = new KubernetesObjectBuilder<V1PodSecurityContext>();
        securityContext(b);

        builder.Add(x=>x.SecurityContext = b.Build());
        return builder;
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

    public static TBuilder WithName<TBuilder>(this TBuilder builder, string name)
       where TBuilder : IKubernetesObjectBuilder<V1Container>
    {
        builder.Add(x => x.Name = name);
        return builder;
    }
    public static TBuilder WithImage<TBuilder>(this TBuilder builder, string image)
       where TBuilder : IKubernetesObjectBuilder<V1Container>
    {
        builder.Add(x => x.Image = image);
        return builder;
    }

    public static TBuilder WithResources<TBuilder>(this TBuilder builder, 
        Action<IList<V1ResourceClaim>>? claims = null,
        Action<IDictionary<string, ResourceQuantity>>? limits = null,
        Action<IDictionary<string, ResourceQuantity>>? requests = null
        )
       where TBuilder : IKubernetesObjectBuilder<V1Container>
    {
        var c = new List<V1ResourceClaim>();
        claims?.Invoke(c);
        var l = new Dictionary<string, ResourceQuantity>();
        limits?.Invoke(l);
        var r = new Dictionary<string, ResourceQuantity>();
        requests?.Invoke(r);

        var resources = new V1ResourceRequirements
        {
            Claims = claims is null ? null : c,
            Limits = limits is null ? null : l,
            Requests = requests is null ? null : r,
        };
        builder.Add(x => x.Resources = resources);
        return builder;
    }
    public static TBuilder AddEnvFromObjectField<TBuilder>(this TBuilder builder, string name, Action<V1ObjectFieldSelector> action)
        where TBuilder : IKubernetesObjectBuilder<V1Container>
    {
        return AddEnv(builder, name, action);
    }

    public static TBuilder AddEnvFromSecretKey<TBuilder>(this TBuilder builder, string name, Action<V1ConfigMapKeySelector> action)
        where TBuilder : IKubernetesObjectBuilder<V1Container>
    {
        return AddEnv(builder, name, action);
    }

    public static TBuilder AddEnvFromConfigMapKey<TBuilder>(this TBuilder builder, string name, Action<V1SecretKeySelector> action)
        where TBuilder : IKubernetesObjectBuilder<V1Container>
    {
        return AddEnv(builder, name, action);
    }

    public static TBuilder AddEnvFromResourceField<TBuilder>(this TBuilder builder, string name, Action<V1ResourceFieldSelector> action)
        where TBuilder : IKubernetesObjectBuilder<V1Container>
    {
        return AddEnv(builder, name, action);
    }

    public static TBuilder WithSecurityContext<TBuilder>(this TBuilder builder, Action<IKubernetesObjectBuilder<V1SecurityContext>> securityContext)
       where TBuilder : IKubernetesObjectBuilder<V1Container>
    {
        var b = new KubernetesObjectBuilder<V1SecurityContext>();
        securityContext(b);

        builder.Add(x=>x.SecurityContext = b.Build());
        return builder;
    }
    

    public static TBuilder AllowPrivilegeEscalation<TBuilder>(this TBuilder builder, bool allowPrivilegeEscalation = true)
       where TBuilder : IKubernetesObjectBuilder<V1SecurityContext>
    {
        builder.Add(x => x.AllowPrivilegeEscalation = allowPrivilegeEscalation);
        return builder;
    }
    public static TBuilder RunAsRoot<TBuilder>(this TBuilder builder, bool runAsRoot = true)
       where TBuilder : IKubernetesObjectBuilder<V1SecurityContext>
    {
        builder.Add(x => x.RunAsNonRoot = runAsRoot);
        return builder;
    }
    public static TBuilder RunAsUser<TBuilder>(this TBuilder builder, int userId)
       where TBuilder : IKubernetesObjectBuilder<V1SecurityContext>
    {
        builder.Add(x => x.RunAsUser = userId);
        return builder;
    }

    public static TBuilder RunAsGroup<TBuilder>(this TBuilder builder, int groupId)
      where TBuilder : IKubernetesObjectBuilder<V1SecurityContext>
    {
        builder.Add(x => x.RunAsGroup = groupId);
        return builder;
    }

    public static TBuilder WithCapabilities<TBuilder>(this TBuilder builder, Action<IKubernetesObjectBuilder<V1Capabilities>> capabilities)
      where TBuilder : IKubernetesObjectBuilder<V1SecurityContext>
    {
        var b = new KubernetesObjectBuilder<V1Capabilities>();
        capabilities(b);
        builder.Add(x => x.Capabilities = b.Build());
        return builder;
    }

    public static TBuilder WithDrop<TBuilder>(this TBuilder builder, params string[] capability)
      where TBuilder : IKubernetesObjectBuilder<V1Capabilities>
    {
        builder.Add(x => x.Drop = capability);
        return builder;
    }

    public static TBuilder AddEnv<TBuilder>(this TBuilder builder, string name, string value)
       where TBuilder : IKubernetesObjectBuilder<V1Container>
    {
        builder.Add(x => {
            x.Env ??= [];
            x.Env.Add(new()
            {
                Name = name,
                Value = value
            });
        });
        return builder;
    }

    public static TBuilder AddEnv<TBuilder, T>(this TBuilder builder, string name, Action<T> action)
        where TBuilder : IKubernetesObjectBuilder<V1Container>
        where T: new()
    {
        var value = new T();
        action(value);
        var valueFrom = value switch
        {
            V1ObjectFieldSelector fieldRef => new V1EnvVarSource() { FieldRef = fieldRef },
            V1SecretKeySelector secretKeyRef => new V1EnvVarSource() { SecretKeyRef = secretKeyRef },
            V1ResourceFieldSelector resourceFieldRef => new V1EnvVarSource() { ResourceFieldRef= resourceFieldRef },
            V1ConfigMapKeySelector configMapKeyRef => new V1EnvVarSource() {  ConfigMapKeyRef = configMapKeyRef },
            _ => throw new InvalidOperationException()
        };

        builder.Add(x => {
            x.Env ??= [];
            x.Env.Add(new()
            {
                Name = name,
                ValueFrom = valueFrom
            });
        });
        return builder;
    }
    
    public static TBuilder WithTerminationGracePeriodSeconds<TBuilder>(this TBuilder builder, int terminationGracePeriodSeconds)
        where TBuilder : IKubernetesObjectBuilder<V1PodSpec>
    {
        builder.Add(x =>
        {
            x.TerminationGracePeriodSeconds = terminationGracePeriodSeconds;
        });
        return builder;
    }
}
