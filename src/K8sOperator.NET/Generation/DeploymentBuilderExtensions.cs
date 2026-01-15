using k8s.Models;

namespace K8sOperator.NET.Generation;

/// <summary>
/// Provides extension methods for building Kubernetes Deployment objects.
/// </summary>
public static class DeploymentBuilderExtensions
{

    /// <summary>
    /// Configures the spec section of the deployment.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <returns>A builder for configuring the deployment spec.</returns>
    public static IKubernetesObjectBuilder<V1DeploymentSpec> WithSpec<TBuilder>(this TBuilder builder)
        where TBuilder : IKubernetesObjectBuilder<V1Deployment>
    {
        var specBuilder = new KubernetesObjectBuilder<V1DeploymentSpec>();
        builder.Add(x => x.Spec = specBuilder.Build());
        return specBuilder;
    }

    /// <summary>
    /// Sets the number of replicas for the deployment.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="replicas">The number of replicas. Defaults to 1.</param>
    /// <returns>The configured builder.</returns>
    public static TBuilder WithReplicas<TBuilder>(this TBuilder builder, int replicas = 1)
        where TBuilder : IKubernetesObjectBuilder<V1DeploymentSpec>
    {
        builder.Add(x => x.Replicas = replicas);
        return builder;
    }

    /// <summary>
    /// Sets the revision history limit for the deployment.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="revisionHistoryLimit">The revision history limit. Defaults to 0.</param>
    /// <returns>The configured builder.</returns>
    public static TBuilder WithRevisionHistory<TBuilder>(this TBuilder builder, int revisionHistoryLimit = 0)
        where TBuilder : IKubernetesObjectBuilder<V1DeploymentSpec>
    {
        builder.Add(x => x.RevisionHistoryLimit = revisionHistoryLimit);
        return builder;
    }

    /// <summary>
    /// Configures the selector for the deployment, allowing selection of resources based on labels and expressions.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="matchExpressions">An action to configure match expressions.</param>
    /// <param name="matchLabels">An action to configure match labels.</param>
    /// <returns>The configured builder.</returns>
    public static TBuilder WithSelector<TBuilder>(this TBuilder builder,
        Action<IList<V1LabelSelectorRequirement>>? matchExpressions = null,
        Action<Dictionary<string, string>>? matchLabels = null)
        where TBuilder : IKubernetesObjectBuilder<V1DeploymentSpec>
    {
        var labels = new Dictionary<string, string>();
        matchLabels?.Invoke(labels);

        var expressions = new List<V1LabelSelectorRequirement>();
        matchExpressions?.Invoke(expressions);

        builder.Add(x => x.Selector = new()
        {
            MatchLabels = matchLabels is null ? null : labels,
            MatchExpressions = matchExpressions is null ? null : expressions
        });
        return builder;
    }

    /// <summary>
    /// Configures the template section of the deployment spec.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <returns>A builder for configuring the pod template spec.</returns>
    public static IKubernetesObjectBuilder<V1PodTemplateSpec> WithTemplate<TBuilder>(this TBuilder builder)
        where TBuilder : IKubernetesObjectBuilder<V1DeploymentSpec>
    {
        var podBuilder = new KubernetesObjectBuilderWithMetaData<V1PodTemplateSpec>();
        builder.Add(x => x.Template = podBuilder.Build());
        return podBuilder;
    }

    /// <summary>
    /// Configures the pod spec section within the pod template.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <returns>A builder for configuring the pod spec.</returns>
    public static IKubernetesObjectBuilder<V1PodSpec> WithPod<TBuilder>(this TBuilder builder)
        where TBuilder : IKubernetesObjectBuilder<V1PodTemplateSpec>
    {
        var podBuilder = new KubernetesObjectBuilder<V1PodSpec>();
        builder.Add(x => x.Spec = podBuilder.Build());
        return podBuilder;
    }



    /// <summary>
    /// Adds a container to the pod spec.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <returns>A builder for configuring the container.</returns>
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

    /// <summary>
    /// Sets the name of the container.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="name">The name of the container.</param>
    /// <returns>The configured builder.</returns>
    public static TBuilder WithName<TBuilder>(this TBuilder builder, string name)
       where TBuilder : IKubernetesObjectBuilder<V1Container>
    {
        builder.Add(x => x.Name = name);
        return builder;
    }

    /// <summary>
    /// Sets the image of the container.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="image">The image of the container.</param>
    /// <returns>The configured builder.</returns>
    public static TBuilder WithImage<TBuilder>(this TBuilder builder, string image)
       where TBuilder : IKubernetesObjectBuilder<V1Container>
    {
        builder.Add(x => x.Image = image);
        return builder;
    }

    /// <summary>
    /// Configures the resource requirements for the container.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="claims">An action to configure resource claims.</param>
    /// <param name="limits">An action to configure resource limits.</param>
    /// <param name="requests">An action to configure resource requests.</param>
    /// <returns>The configured builder.</returns>
    public static TBuilder WithResources<TBuilder>(this TBuilder builder,
        Action<IList<Corev1ResourceClaim>>? claims = null,
        Action<IDictionary<string, ResourceQuantity>>? limits = null,
        Action<IDictionary<string, ResourceQuantity>>? requests = null
        )
       where TBuilder : IKubernetesObjectBuilder<V1Container>
    {
        var c = new List<Corev1ResourceClaim>();
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

    /// <summary>
    /// Adds an environment variable from an object field to the container.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="name">The name of the environment variable.</param>
    /// <param name="action">An action to configure the object field selector.</param>
    /// <returns>The configured builder.</returns>
    public static TBuilder AddEnvFromObjectField<TBuilder>(this TBuilder builder, string name, Action<V1ObjectFieldSelector> action)
        where TBuilder : IKubernetesObjectBuilder<V1Container>
    {
        return builder.AddEnv(name, action);
    }

    /// <summary>
    /// Adds an environment variable to the container that sources its value from a ConfigMap key.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="name">The name of the environment variable.</param>
    /// <param name="action">An action to configure the ConfigMap key selector.</param>
    /// <returns>The configured builder.</returns>
    public static TBuilder AddEnvFromSecretKey<TBuilder>(this TBuilder builder, string name, Action<V1ConfigMapKeySelector> action)
        where TBuilder : IKubernetesObjectBuilder<V1Container>
    {
        return builder.AddEnv(name, action);
    }

    /// <summary>
    /// Adds an environment variable to the container that sources its value from a secret key.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="name">The name of the environment variable.</param>
    /// <param name="action">An action to configure the secret key selector.</param>
    /// <returns>The configured builder.</returns>
    public static TBuilder AddEnvFromConfigMapKey<TBuilder>(this TBuilder builder, string name, Action<V1ConfigMapKeySelector> action)
        where TBuilder : IKubernetesObjectBuilder<V1Container>
    {
        return builder.AddEnv(name, action);
    }

    /// <summary>
    /// Adds an environment variable to the container that sources its value from a resource field.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="name">The name of the environment variable.</param>
    /// <param name="action">An action to configure the resource field selector.</param>
    /// <returns>The configured builder.</returns>
    public static TBuilder AddEnvFromResourceField<TBuilder>(this TBuilder builder, string name, Action<V1ResourceFieldSelector> action)
        where TBuilder : IKubernetesObjectBuilder<V1Container>
    {
        return builder.AddEnv(name, action);
    }

    /// <summary>
    /// Configures the security context for the container.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="securityContext">An action to configure the security context.</param>
    /// <returns>The configured builder.</returns>
    public static TBuilder WithSecurityContext<TBuilder>(this TBuilder builder, Action<IKubernetesObjectBuilder<V1SecurityContext>> securityContext)
       where TBuilder : IKubernetesObjectBuilder<V1Container>
    {
        var b = new KubernetesObjectBuilder<V1SecurityContext>();
        securityContext(b);

        builder.Add(x => x.SecurityContext = b.Build());
        return builder;
    }

    /// <summary>
    /// Configures the security context for the pod.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="securityContext">An action to configure the security context.</param>
    /// <returns>The configured builder.</returns>
    public static TBuilder WithSecurityContext<TBuilder>(this TBuilder builder, Action<IKubernetesObjectBuilder<V1PodSecurityContext>> securityContext)
        where TBuilder : IKubernetesObjectBuilder<V1PodSpec>
    {
        var b = new KubernetesObjectBuilder<V1PodSecurityContext>();
        securityContext(b);

        builder.Add(x => x.SecurityContext = b.Build());
        return builder;
    }

    /// <summary>
    /// Configures whether the container allows privilege escalation.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="allowPrivilegeEscalation">A value indicating whether to allow privilege escalation. Defaults to true.</param>
    /// <returns>The configured builder.</returns>
    public static TBuilder AllowPrivilegeEscalation<TBuilder>(this TBuilder builder, bool allowPrivilegeEscalation = true)
       where TBuilder : IKubernetesObjectBuilder<V1SecurityContext>
    {
        builder.Add(x => x.AllowPrivilegeEscalation = allowPrivilegeEscalation);
        return builder;
    }

    /// <summary>
    /// Configures whether the container should run as a non-root user.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="runAsRoot">A value indicating whether to run as a root user. Defaults to true.</param>
    /// <returns>The configured builder.</returns>
    public static TBuilder RunAsRoot<TBuilder>(this TBuilder builder, bool runAsRoot = true)
       where TBuilder : IKubernetesObjectBuilder<V1SecurityContext>
    {
        builder.Add(x => x.RunAsNonRoot = runAsRoot);
        return builder;
    }

    /// <summary>
    /// Configures the user ID that the container should run as.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="userId">The user ID to run the container as.</param>
    /// <returns>The configured builder.</returns>
    public static TBuilder RunAsUser<TBuilder>(this TBuilder builder, int userId)
       where TBuilder : IKubernetesObjectBuilder<V1SecurityContext>
    {
        builder.Add(x => x.RunAsUser = userId);
        return builder;
    }

    /// <summary>
    /// Configures the group ID that the container should run as.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="groupId">The group ID to run the container as.</param>
    /// <returns>The configured builder.</returns>
    public static TBuilder RunAsGroup<TBuilder>(this TBuilder builder, int groupId)
      where TBuilder : IKubernetesObjectBuilder<V1SecurityContext>
    {
        builder.Add(x => x.RunAsGroup = groupId);
        return builder;
    }

    /// <summary>
    /// Configures the security capabilities for the container.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="capabilities">An action to configure the security capabilities.</param>
    /// <returns>The configured builder.</returns>
    public static TBuilder WithCapabilities<TBuilder>(this TBuilder builder, Action<IKubernetesObjectBuilder<V1Capabilities>> capabilities)
      where TBuilder : IKubernetesObjectBuilder<V1SecurityContext>
    {
        var b = new KubernetesObjectBuilder<V1Capabilities>();
        capabilities(b);
        builder.Add(x => x.Capabilities = b.Build());
        return builder;
    }

    /// <summary>
    /// Configures the security capabilities to drop from the container.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="capability">An array of capabilities to drop.</param>
    /// <returns>The configured builder.</returns>
    public static TBuilder WithDrop<TBuilder>(this TBuilder builder, params string[] capability)
      where TBuilder : IKubernetesObjectBuilder<V1Capabilities>
    {
        builder.Add(x => x.Drop = capability);
        return builder;
    }

    /// <summary>
    /// Adds an environment variable to the container with the specified name and value.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="name">The name of the environment variable.</param>
    /// <param name="value">The value of the environment variable.</param>
    /// <returns>The configured builder.</returns>
    public static TBuilder AddEnv<TBuilder>(this TBuilder builder, string name, string value)
       where TBuilder : IKubernetesObjectBuilder<V1Container>
    {
        builder.Add(x =>
        {
            x.Env ??= [];
            x.Env.Add(new()
            {
                Name = name,
                Value = value
            });
        });
        return builder;
    }

    /// <summary>
    /// Adds an environment variable to the container with the specified name, 
    /// and configures its value from a Kubernetes resource, such as a field selector, secret key, or config map key.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <typeparam name="T">The type of the Kubernetes resource used to source the environment variable's value.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="name">The name of the environment variable.</param>
    /// <param name="action">An action to configure the resource selector.</param>
    /// <returns>The configured builder.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the resource type is not supported.</exception>
    public static TBuilder AddEnv<TBuilder, T>(this TBuilder builder, string name, Action<T> action)
        where TBuilder : IKubernetesObjectBuilder<V1Container>
        where T : new()
    {
        var value = new T();
        action(value);
        var valueFrom = value switch
        {
            V1ObjectFieldSelector fieldRef => new V1EnvVarSource() { FieldRef = fieldRef },
            V1SecretKeySelector secretKeyRef => new V1EnvVarSource() { SecretKeyRef = secretKeyRef },
            V1ResourceFieldSelector resourceFieldRef => new V1EnvVarSource() { ResourceFieldRef = resourceFieldRef },
            V1ConfigMapKeySelector configMapKeyRef => new V1EnvVarSource() { ConfigMapKeyRef = configMapKeyRef },
            _ => throw new InvalidOperationException()
        };

        builder.Add(x =>
        {
            x.Env ??= [];
            x.Env.Add(new()
            {
                Name = name,
                ValueFrom = valueFrom
            });
        });
        return builder;
    }

    /// <summary>
    /// Configures the termination grace period in seconds for the pod.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="terminationGracePeriodSeconds">The duration in seconds that Kubernetes waits before forcefully terminating the pod.</param>
    /// <returns>The configured builder.</returns>
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
