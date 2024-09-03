using K8sOperator.NET;

namespace SimpleOperator.Projects;

public class TestItemController : Controller<TestItem>
{
    public override Task AddOrModifyAsync(TestItem resource, CancellationToken cancellationToken)
    {
        return base.AddOrModifyAsync(resource, cancellationToken);
    }

    public override Task BookmarkAsync(TestItem resource, CancellationToken cancellationToken)
    {
        return base.BookmarkAsync(resource, cancellationToken);
    }

    public override Task DeleteAsync(TestItem resource, CancellationToken cancellationToken)
    {
        return base.DeleteAsync(resource, cancellationToken);
    }

    public override Task ErrorAsync(TestItem resource, CancellationToken cancellationToken)
    {
        return base.ErrorAsync(resource, cancellationToken);
    }

    public override Task FinalizeAsync(TestItem resource, CancellationToken cancellationToken)
    {
        return base.FinalizeAsync(resource, cancellationToken);
    }
}
