using k8s.Models;
using K8sOperator.NET.Generator.Builders;

namespace K8sOperator.NET.Generators.Builders;

/// <summary>
/// Provides extension methods for building Kubernetes ClusterRoles.
/// </summary>
public static class ClusterRoleBuilderExtensions
{
    /// <summary>
    /// Adds a policy rule to the ClusterRole being built.
    /// </summary>
    /// <param name="builder">The builder instance for the ClusterRole.</param>
    /// <returns>A builder for configuring the policy rule.</returns>
    public static IKubernetesObjectBuilder<V1PolicyRule> AddRule(this IKubernetesObjectBuilder<V1ClusterRole> builder)
    {
        var b = new PolicyRuleBuilder();
        builder.Add(x =>
        {
            x.Rules ??= [];
            x.Rules.Add(b.Build());
        });
        return b;
    }
}
