using k8s.Models;

namespace K8sOperator.NET.Generator.Builders;

public static class ClusterRoleBindingBuilderExtensions
{
    public static TBuilder WithRoleRef<TBuilder>(this TBuilder builder, string apiGroup, string kind, string name)
        where TBuilder : IKubernetesObjectBuilder<V1ClusterRoleBinding>
    {
        builder.Add(x =>
        {
            x.RoleRef = new V1RoleRef(apiGroup, kind, name);
        });

        return builder;
    }

    public static TBuilder WithSubject<TBuilder>(this TBuilder builder, string kind, string name, string? apiGroup = null, string? ns = null)
        where TBuilder : IKubernetesObjectBuilder<V1ClusterRoleBinding>
    {
        builder.Add(x =>
        {
            x.Subjects ??= [];
            x.Subjects.Add(new Rbacv1Subject(kind, name, apiGroup, ns));
        });

        return builder;
    }
}
