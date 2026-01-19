using k8s;
using k8s.Autorest;
using k8s.Models;
using K8sOperator.NET.Generation;
using System.Net;

namespace K8sOperator.NET;

public class LeaderElectionOptions
{
    public string LeaseName { get; set; } = string.Empty;
    public string LeaseNamespace { get; set; } = string.Empty;
    public bool Enabled { get; set; } = false;
    public TimeSpan LeaseDuration { get; set; } = TimeSpan.FromSeconds(15);
    public TimeSpan RenewInterval { get; set; } = TimeSpan.FromSeconds(5);
    public TimeSpan RetryPeriod { get; set; } = TimeSpan.FromSeconds(2);
}

public class LeaderElectionService(IKubernetes kubernetes, LeaderElectionOptions options) : ILeaderElectionService
{
    private readonly LeaderElectionOptions _options = options;
    private readonly string _identity = $"{Environment.MachineName}-{Guid.NewGuid()}";
    private Task? _renewalTask;

    public bool IsLeader { get; internal set; }

    public async Task StartAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (await TryAcquireLeaseAsync(stoppingToken))
            {
                IsLeader = true;
                _renewalTask = RenewLeaseLoopAsync(stoppingToken);
                await _renewalTask;
            }
            else
            {
                IsLeader = false;
                await Task.Delay(_options.RetryPeriod, stoppingToken);
            }
        }
    }

    private async Task RenewLeaseLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && IsLeader)
        {
            await Task.Delay(_options.RenewInterval, cancellationToken);

            try
            {
                if (!await TryUpdateLeaseAsync(cancellationToken))
                {
                    IsLeader = false;
                    break;
                }
            }
            catch (Exception)
            {
                IsLeader = false;
                break;
            }
        }
    }


    private async Task<bool> TryAcquireLeaseAsync(CancellationToken cancellationToken)
    {
        var leaseBuilder = KubernetesObjectBuilder.Create<V1Lease>();

        leaseBuilder.WithName(_options.LeaseName)
                    .WithNamespace(_options.LeaseNamespace)
                    .WithSpecs()
                        .WithHolderIdentity(_identity)
                        .WithLeaseDuration((int)_options.LeaseDuration.TotalSeconds)
                        .WithAcquireTime(DateTime.UtcNow)
                        .WithRenewTime(DateTime.UtcNow);

        var lease = leaseBuilder.Build();

        try
        {
            await kubernetes.CoordinationV1.CreateNamespacedLeaseAsync(
                lease,
                _options.LeaseNamespace,
                cancellationToken: cancellationToken);
            return true;
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == HttpStatusCode.Conflict)
        {
            // Lease already exists, try to acquire
            return await TryUpdateLeaseAsync(cancellationToken);
        }
    }

    private async Task<bool> TryUpdateLeaseAsync(CancellationToken cancellationToken)
    {
        try
        {
            var existingLease = await kubernetes.CoordinationV1.ReadNamespacedLeaseAsync(
                _options.LeaseName,
                _options.LeaseNamespace,
                cancellationToken: cancellationToken);

            var now = DateTime.UtcNow;
            var renewTime = existingLease.Spec.RenewTime?.ToUniversalTime() ?? DateTime.MinValue;
            var leaseDuration = TimeSpan.FromSeconds(existingLease.Spec.LeaseDurationSeconds ?? 15);
            var isExpired = now > renewTime.Add(leaseDuration);
            var isCurrentHolder = existingLease.Spec.HolderIdentity == _identity;

            if (!isExpired && !isCurrentHolder)
            {
                return false;
            }

            existingLease.Spec.HolderIdentity = _identity;
            existingLease.Spec.RenewTime = now;

            if (!isCurrentHolder)
            {
                existingLease.Spec.AcquireTime = now;
                existingLease.Spec.LeaseTransitions = (existingLease.Spec.LeaseTransitions ?? 0) + 1;
            }

            await kubernetes.CoordinationV1.ReplaceNamespacedLeaseAsync(
                existingLease,
                _options.LeaseName,
                _options.LeaseNamespace,
                cancellationToken: cancellationToken);

            return true;
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == HttpStatusCode.Conflict)
        {
            return false;
        }
    }
}
