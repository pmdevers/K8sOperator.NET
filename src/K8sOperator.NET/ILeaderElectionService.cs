namespace K8sOperator.NET;

public interface ILeaderElectionService
{
    bool IsLeader { get; }
    public Task StartAsync(CancellationToken stoppingToken);
}

internal class NoopLeaderElectionService() : ILeaderElectionService
{
    public bool IsLeader => true;
    public Task StartAsync(CancellationToken stoppingToken) => Task.CompletedTask;
}
