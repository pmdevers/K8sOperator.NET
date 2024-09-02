using k8s.Models;

namespace K8sOperator.NET.Generator.Builders;

public static class PolicyRuleBuilderExtensions
{
    public static TBuilder WithGroups<TBuilder>(this TBuilder builder, params string[] groups)
        where TBuilder : IKubernetesObjectBuilder<V1PolicyRule>
    {
        builder.Add(x => x.ApiGroups = groups);
        return builder;
    }

    public static TBuilder WithVerbs<TBuilder>(this TBuilder builder, params string[] verbs)
        where TBuilder : IKubernetesObjectBuilder<V1PolicyRule>
    {
        builder.Add(x => x.Verbs = verbs);
        return builder;
    }

    public static TBuilder WithResources<TBuilder>(this TBuilder builder, params string[] resources)
        where TBuilder : IKubernetesObjectBuilder<V1PolicyRule>
    {
        builder.Add(x => x.Resources = resources);
        return builder;
    }
}
