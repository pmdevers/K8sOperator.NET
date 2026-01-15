using k8s;
using k8s.Models;

namespace K8sOperator.NET.Generation;

internal class ClusterRoleBindingBuilder : KubernetesObjectBuilderWithMetaData<V1ClusterRoleBinding>
{
    public override V1ClusterRoleBinding Build()
    {
        var role = base.Build();
        role.Initialize();
        return role;
    }
}
