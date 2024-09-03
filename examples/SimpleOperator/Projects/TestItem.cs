using k8s.Models;
using K8sOperator.NET.Generator.Builders;
using K8sOperator.NET.Models;
using static SimpleOperator.Projects.TestItem;

namespace SimpleOperator.Projects;

[KubernetesEntity(Group = "sonarcloud.io", ApiVersion = "v1alpha1", Kind = "TestItem", PluralName = "testitems")]
public class TestItem : CustomResource<TestItemSpec, TestItemStatus>
{
    public class TestItemSpec
    {
        public Scope Scope { get; set; }
        public string? String { get; set; }
    }

    public class TestItemStatus
    {

    }
}
