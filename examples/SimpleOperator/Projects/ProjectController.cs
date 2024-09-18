using K8sOperator.NET;
using K8sOperator.NET.Metadata;

namespace SimpleOperator.Projects;

[Namespace("default")]
public class ProjectController(ILoggerFactory logger) : Controller<Project>
{
    private readonly ILogger _logger = logger.CreateLogger<ProjectController>();

    public override Task AddOrModifyAsync(Project resource, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Controller AddOrModify received.");

        resource.Metadata.Labels = new Dictionary<string, string>
        {
            { "created", "created" }
        };

        resource.Status.Result = "HEHE";

        return base.AddOrModifyAsync(resource, cancellationToken);
    }

    public override Task DeleteAsync(Project resource, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Controller Delete received.");
        return base.DeleteAsync(resource, cancellationToken);
    }

    public override Task FinalizeAsync(Project resource, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Controller Finalize received.");
        return base.FinalizeAsync(resource, cancellationToken);
    }
}
