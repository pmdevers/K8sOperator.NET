using k8s.Models;

namespace K8sOperator.NET.Generation;

/// <summary>
/// Provides extension methods for building Kubernetes ClusterRoleBindings.
/// </summary>
public static class ClusterRoleBindingBuilderExtensions
{

    /// <summary>
    /// Configures the RoleRef for the ClusterRoleBinding.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="apiGroup">The API group of the role.</param>
    /// <param name="kind">The kind of the role.</param>
    /// <param name="name">The name of the role.</param>
    /// <returns>The configured builder.</returns>
    public static TBuilder WithRoleRef<TBuilder>(this TBuilder builder, string apiGroup, string kind, string name)
        where TBuilder : IKubernetesObjectBuilder<V1ClusterRoleBinding>
    {
        builder.Add(x =>
        {
            x.RoleRef = new V1RoleRef() { ApiGroup = apiGroup, Kind = kind, Name = name };
        });

        return builder;
    }

    /// <summary>
    /// Adds a subject to the ClusterRoleBinding.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="kind">The kind of the subject (e.g., User, Group, ServiceAccount).</param>
    /// <param name="name">The name of the subject.</param>
    /// <param name="apiGroup">The API group of the subject, if applicable.</param>
    /// <param name="ns">The namespace of the subject, if applicable.</param>
    /// <returns>The configured builder.</returns>
    public static TBuilder WithSubject<TBuilder>(this TBuilder builder, string kind, string name, string? apiGroup = null, string? ns = null)
        where TBuilder : IKubernetesObjectBuilder<V1ClusterRoleBinding>
    {
        builder.Add(x =>
        {
            x.Subjects ??= [];
            x.Subjects.Add(new Rbacv1Subject() { Kind = kind, Name = name, ApiGroup = apiGroup, NamespaceProperty = ns });
        });

        return builder;
    }
}
