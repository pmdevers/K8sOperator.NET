using k8s;
using k8s.Models;
using K8sOperator.NET;

namespace SimpleOperator.Projects;


public class ProjectController(IKubernetes client, ILoggerFactory logger) : Controller<Project>
{
    private readonly ILogger _logger = logger.CreateLogger<ProjectController>();

    public override Task AddOrModifyAsync(Project resource, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Controller AddOrModify received.");
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
