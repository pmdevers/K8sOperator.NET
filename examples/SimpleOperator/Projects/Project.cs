using k8s.Models;
using K8sOperator.NET.Models;

namespace SimpleOperator.Projects;

[KubernetesEntity(ApiVersion = "v1alpha1", Group = "sonarcloud.io", Kind = "Project", PluralName = "projects")]
public class Project : CustomResource<Project.Specs, Project.ProjectStatus>
{
    public class Specs
    {
        public string Name { get; set; } = string.Empty;
        public string Organization { get; set; } = string.Empty;
        public string Project { get; set; } = string.Empty;
    }

    public class ProjectStatus
    {
        public string Result { get; set; } = string.Empty;
    }
}
