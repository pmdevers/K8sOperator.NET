using K8sOperator.NET;
using SimpleOperator.Resources;

namespace SimpleOperator.Controllers;

public class TodoController : OperatorController<TodoItem>
{
    private readonly ILogger<TodoController> _logger;

    public TodoController(ILogger<TodoController> logger)
    {
        _logger = logger;
    }

    public override async Task AddOrModifyAsync(TodoItem resource, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing TodoItem: {Name} - Title: {Title}, Priority: {Priority}",
            resource.Metadata.Name,
            resource.Spec.Title,
            resource.Spec.Priority);

        // Initialize status if needed
        if (resource.Status == null)
        {
            resource.Status = new TodoItem.TodoStatus();
        }

        // Update reconciliation count
        resource.Status.ReconciliationCount++;

        // Business logic: Auto-complete if due date is passed
        if (resource.Spec.DueDate.HasValue &&
            resource.Spec.DueDate.Value < DateTime.UtcNow &&
            resource.Status.State != "completed")
        {
            resource.Status.State = "overdue";
            resource.Status.Message = $"Task is overdue by {(DateTime.UtcNow - resource.Spec.DueDate.Value).Days} days";
            _logger.LogWarning("TodoItem {Name} is overdue!", resource.Metadata.Name);
        }
        else if (resource.Status.State == "pending")
        {
            resource.Status.State = "in-progress";
            resource.Status.Message = "Task is being processed";
            _logger.LogInformation("TodoItem {Name} moved to in-progress", resource.Metadata.Name);
        }

        // Simulate some async work
        await Task.Delay(100, cancellationToken);
    }

    public override async Task DeleteAsync(TodoItem resource, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Deleting TodoItem: {Name}. Final state was: {State}",
            resource.Metadata.Name,
            resource.Status?.State ?? "unknown");

        // Cleanup logic here (e.g., remove external resources)
        await Task.CompletedTask;
    }

    public override async Task FinalizeAsync(TodoItem resource, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Finalizing TodoItem: {Name}",
            resource.Metadata.Name);

        // Perform cleanup before resource is completely deleted
        // For example: remove related resources, notify external systems, etc.

        await Task.CompletedTask;
    }

    public override async Task ErrorAsync(TodoItem resource, CancellationToken cancellationToken)
    {
        _logger.LogError(
            "Error occurred for TodoItem: {Name}",
            resource.Metadata.Name);

        if (resource.Status != null)
        {
            resource.Status.State = "error";
            resource.Status.Message = "An error occurred during reconciliation";
        }

        await Task.CompletedTask;
    }
}
