using k8s.Models;

namespace K8sOperator.NET.Generation;

/// <summary>
/// Provides extension methods for building Kubernetes policy rules.
/// </summary>
public static class PolicyRuleBuilderExtensions
{
    /// <summary>
    /// Sets the API groups for the policy rule.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="groups">The API groups to assign to the policy rule.</param>
    /// <returns>The configured builder.</returns>
    public static TBuilder WithGroups<TBuilder>(this TBuilder builder, params string[] groups)
        where TBuilder : IKubernetesObjectBuilder<V1PolicyRule>
    {
        builder.Add(x => x.ApiGroups = groups);
        return builder;
    }

    /// <summary>
    /// Sets the verbs for the policy rule, defining the actions that can be performed.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="verbs">The verbs to assign to the policy rule.</param>
    /// <returns>The configured builder.</returns>
    public static TBuilder WithVerbs<TBuilder>(this TBuilder builder, params string[] verbs)
        where TBuilder : IKubernetesObjectBuilder<V1PolicyRule>
    {
        builder.Add(x => x.Verbs = verbs);
        return builder;
    }

    /// <summary>
    /// Sets the resources for the policy rule, specifying the resources that the rule applies to.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="resources">The resources to assign to the policy rule.</param>
    /// <returns>The configured builder.</returns>
    public static TBuilder WithResources<TBuilder>(this TBuilder builder, params string[] resources)
        where TBuilder : IKubernetesObjectBuilder<V1PolicyRule>
    {
        builder.Add(x => x.Resources = resources);
        return builder;
    }
}
