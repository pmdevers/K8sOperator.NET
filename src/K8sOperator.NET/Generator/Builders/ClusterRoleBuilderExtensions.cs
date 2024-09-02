using k8s.Models;

namespace K8sOperator.NET.Generator.Builders;

public static class ClusterRoleBuilderExtensions
{
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
