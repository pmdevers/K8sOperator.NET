using k8s.Models;
using K8sOperator.NET.Models;

namespace K8sOperator.NET.ProjectTemplate.Todos;

/// <summary>
/// Represents a todoitem.
/// </summary>
[KubernetesEntity(Group = "todo.io", ApiVersion = "v1alpha1", Kind = "TodoItem", PluralName = "todoitems")]
public class TodoItem : CustomResource<TodoItem.Specs, TodoItem.State>
{
    /// <summary>
    /// The spec of the todoitem.
    /// </summary>
    public class Specs
    {
        /// <summary>
        /// Gets or sets the title of the todoitem.
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// The state of the todoitem.
    /// </summary>
    public class State
    {
        /// <summary>
        /// Gets or sets a value indicating whether the todoitem is complete.
        /// </summary>
        public bool IsComplete { get; set; }
    }
}
