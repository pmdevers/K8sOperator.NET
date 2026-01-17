using k8s.Models;

namespace K8sOperator.NET.Tests.Fixtures;

[KubernetesEntity(Group = "unittest", ApiVersion = "v1", Kind = "TestResource", PluralName = "testresources")]
public class TestResource : CustomResource<TestResource.TestSpec, TestResource.TestStatus>
{
    public class TestStatus
    {
        public string Status { get; set; } = string.Empty;
    }

    public class TestSpec
    {
        public string Property { get; set; } = string.Empty;
    }
}
