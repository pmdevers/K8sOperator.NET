namespace K8sOperator.NET;

public interface ILeaderElectionService
{
    bool IsLeader { get; }
    Task StartAsync(CancellationToken stoppingToken);

    /// <summary>
    /// Waits until leadership is acquired.
    /// </summary>
    Task WaitForLeadershipAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Waits until leadership is lost.
    /// </summary>
    Task WaitForLeadershipLostAsync(CancellationToken cancellationToken);
}

internal class NoopLeaderElectionService() : ILeaderElectionService
{
    public bool IsLeader => true;
    public Task StartAsync(CancellationToken stoppingToken) => Task.CompletedTask;
    public Task WaitForLeadershipAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task WaitForLeadershipLostAsync(CancellationToken cancellationToken) => Task.Delay(Timeout.Infinite, cancellationToken);
}
