using k8s;
using K8sOperator.NET;
using K8sOperator.NET.Builder;
using K8sOperator.NET.Commands;
using K8sOperator.NET.Generation;
using K8sOperator.NET.Metadata;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace K8sOperator.NET;

public static class OperatorExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddOperator(Action<OperatorBuilder>? configure = null)
        {
            var builder = new OperatorBuilder();
            configure?.Invoke(builder);

            services.TryAddSingleton(sp =>
            {
                var ds = new CommandDatasource(sp);

                ds.Add<HelpCommand>();
                ds.Add<OperatorCommand>();
                ds.Add<InstallCommand>();
                ds.Add<VersionCommand>();
                ds.Add<CreateCommand>();

#if DEBUG
                // Development-only commands
                ds.Add<GenerateLaunchSettingsCommand>();
                ds.Add<GenerateDockerfileCommand>();
#endif

                return ds;
            });

            services.TryAddSingleton(sp =>
            {
                var operatorName = Assembly.GetEntryAssembly()?.GetCustomAttribute<OperatorNameAttribute>()
                    ?? OperatorNameAttribute.Default;

                var dockerImage = Assembly.GetEntryAssembly()?.GetCustomAttribute<DockerImageAttribute>()
                    ?? DockerImageAttribute.Default;

                var ns = Assembly.GetEntryAssembly()?.GetCustomAttribute<NamespaceAttribute>()
                    ?? NamespaceAttribute.Default;

                return new EventWatcherDatasource(sp, [operatorName, dockerImage, ns]);
            });

            services.TryAddSingleton<IKubernetes>((sp) =>
            {
                var config = builder?.KubeConfig
                    ?? KubernetesClientConfiguration.BuildDefaultConfig();
                return new Kubernetes(config);
            });

            services.TryAddSingleton(sp => builder.LeaderElection);
            services.TryAddSingleton(sp =>
            {
                var o = sp.GetRequiredService<LeaderElectionOptions>();
                var type = o.Enabled ? typeof(LeaderElectionService) : typeof(NoopLeaderElectionService);
                return (ILeaderElectionService)ActivatorUtilities.CreateInstance(sp, type);
            });
            services.AddHostedService<OperatorService>();

            return services;
        }
    }

    extension(IApplicationBuilder app)
    {
        public ConventionBuilder<ControllerBuilder> MapController<T>()
            where T : IOperatorController
        {
            var datasource = app.ApplicationServices.GetRequiredService<EventWatcherDatasource>();
            return datasource.Add<T>();
        }
    }

    extension(IHost app)
    {
        public Task RunOperatorAsync()
        {
            var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
            var commandDatasource = app.Services.GetRequiredService<CommandDatasource>();
            var command = commandDatasource.GetCommands(app)
                .FirstOrDefault(Filter)?.Command;

            bool Filter(CommandInfo command)
            {
                var attr = command.Metadata.OfType<OperatorArgumentAttribute>().FirstOrDefault();
                if (attr is null) return false;
                var arg = attr.Argument;
                return args.FirstOrDefault() == arg || args.FirstOrDefault() == $"--{arg}";
            }

            if (command == null)
            {
                var helpCommand = commandDatasource.GetCommands(app)
                    .First(c => c.Command is HelpCommand).Command;
                return helpCommand.RunAsync(args);
            }

            return command.RunAsync(args);
        }
    }
}

public class OperatorBuilder
{
    public static NamespaceAttribute Namespace = Assembly.GetExecutingAssembly().GetCustomAttribute<NamespaceAttribute>() ??
            NamespaceAttribute.Default;
    public static DockerImageAttribute Docker = Assembly.GetExecutingAssembly().GetCustomAttribute<DockerImageAttribute>() ??
            DockerImageAttribute.Default;
    public static OperatorNameAttribute Operator = Assembly.GetExecutingAssembly().GetCustomAttribute<OperatorNameAttribute>() ??
            OperatorNameAttribute.Default;

    public OperatorBuilder()
    {
        LeaderElection = new ObjectBuilder<LeaderElectionOptions>().Add(x =>
        {
            x.LeaseName = $"{Operator.OperatorName}-leader-election";
            x.LeaseNamespace = Namespace.Namespace;
        }).Build();
    }


    public KubernetesClientConfiguration? KubeConfig { get; set; }
    public LeaderElectionOptions LeaderElection { get; set; }

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
