using k8s;
using k8s.Models;

namespace K8sOperator.NET.Generator.Builders;

internal class CustomResourceDefinitionBuilder : KubernetesObjectBuilderWithMetaData<V1CustomResourceDefinition>
{
    public override V1CustomResourceDefinition Build()
    {
        var crd = base.Build();
        crd.Initialize();
        return crd;
    }
}

