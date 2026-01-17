using k8s.Models;
using K8sOperator.NET;

namespace SimpleOperator.Resources;

[KubernetesEntity(Group = "app.example.com", ApiVersion = "v1", Kind = "TodoItem", PluralName = "todoitems")]
public class TodoItem : CustomResource<TodoItem.TodoSpec, TodoItem.TodoStatus>
{
    public class TodoSpec
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = "medium"; // low, medium, high
        public DateTime? DueDate { get; set; }
    }

    public class TodoStatus
    {
        public string State { get; set; } = "pending"; // pending, in-progress, completed
        public DateTime? CompletedAt { get; set; }
        public string Message { get; set; } = string.Empty;
        public int ReconciliationCount { get; set; }
    }
}
