using k8s;
using K8sOperator.NET.Configuration;

namespace K8sOperator.NET;

public class OperatorBuilder
{
    public OperatorBuilder()
    {
        LeaderElection = new LeaderElectionOptions
        {
            Enabled = false
        };
    }

    public KubernetesClientConfiguration? KubeConfig { get; set; }
    public LeaderElectionOptions LeaderElection { get; set; }

    /// <summary>
    /// Operator configuration that can be set programmatically.
    /// This has the highest priority and will override assembly attributes and appsettings.json.
    /// </summary>
    public OperatorConfiguration? OperatorConfiguration { get; set; }

    public void WithKubeConfig(KubernetesClientConfiguration config)
    {
        KubeConfig = config;
    }

    public void WithLeaderElection(Action<LeaderElectionOptions>? actions = null)
    {
        LeaderElection.Enabled = true;
        actions?.Invoke(LeaderElection);
    }
}
