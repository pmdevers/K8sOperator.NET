using k8s.Models;
using K8sOperator.NET.Metadata;
using K8sOperator.NET.Models;
using static SimpleOperator.Projects.TestItem;

namespace SimpleOperator.Projects;

[KubernetesEntity(Group = "operator.io", ApiVersion = "v1alpha1", Kind = "TestItem", PluralName = "testitems")]
public class TestItem : CustomResource<TestItemSpec, TestItemStatus>
{
    public class TestItemSpec
    {
        public EntityScope Scope { get; set; }
        public string? String { get; set; }
    }

    public class TestItemStatus
    {

    }
}
