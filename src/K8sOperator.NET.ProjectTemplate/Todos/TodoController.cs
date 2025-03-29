using K8sOperator.NET.Metadata;

namespace K8sOperator.NET.ProjectTemplate.Todos;

[Namespace("todo-system")]
internal class TodoController : Controller<TodoItem>
{
    public override Task AddOrModifyAsync(TodoItem resource, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Adding or modifying todo item: {resource.Metadata.Name}");
        return Task.CompletedTask;
    }
    public override Task DeleteAsync(TodoItem resource, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Deleting todo item: {resource.Metadata.Name}");
        return Task.CompletedTask;
    }
    public override Task FinalizeAsync(TodoItem resource, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Finalizing todo item: {resource.Metadata.Name}");
        return Task.CompletedTask;
    }
}
