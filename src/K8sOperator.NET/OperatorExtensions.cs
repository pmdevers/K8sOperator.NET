using k8s;
using K8sOperator.NET;
using K8sOperator.NET.Builder;
using K8sOperator.NET.Commands;
using K8sOperator.NET.Configuration;
using K8sOperator.NET.Generation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace K8sOperator.NET;

public static class OperatorExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddOperator(Action<OperatorBuilder>? configure = null)
        {
            var builder = new OperatorBuilder();
            configure?.Invoke(builder);

            // Register operator configuration
            services.TryAddSingleton(sp =>
            {
                var configuration = sp.GetService<IConfiguration>();
                var provider = new OperatorConfigurationProvider(configuration);
                var config = provider.Build();

                // Apply OperatorBuilder overrides if provided
                if (builder.OperatorConfiguration != null)
                {
                    if (!string.IsNullOrEmpty(builder.OperatorConfiguration.OperatorName))
                        config.OperatorName = builder.OperatorConfiguration.OperatorName;
                    if (!string.IsNullOrEmpty(builder.OperatorConfiguration.Namespace))
                        config.Namespace = builder.OperatorConfiguration.Namespace;
                    if (!string.IsNullOrEmpty(builder.OperatorConfiguration.ContainerRegistry))
                        config.ContainerRegistry = builder.OperatorConfiguration.ContainerRegistry;
                    if (!string.IsNullOrEmpty(builder.OperatorConfiguration.ContainerRepository))
                        config.ContainerRepository = builder.OperatorConfiguration.ContainerRepository;
                    if (!string.IsNullOrEmpty(builder.OperatorConfiguration.ContainerTag))
                        config.ContainerTag = builder.OperatorConfiguration.ContainerTag;
                }

                config.Validate();

                return config;
            });

            services.TryAddSingleton(sp =>
            {
                var ds = new CommandDatasource(sp);

                ds.Add<HelpCommand>();
                ds.Add<OperatorCommand>();
                ds.Add<InstallCommand>();
                ds.Add<VersionCommand>();
                ds.Add<CreateCommand>();

                return ds;
            });

            services.TryAddSingleton(sp =>
            {
                // Use OperatorConfiguration directly
                var config = sp.GetRequiredService<OperatorConfiguration>();
                return new EventWatcherDatasource(sp, config);
            });

            services.TryAddSingleton<IKubernetes>((sp) =>
            {
                var config = builder?.KubeConfig
                    ?? KubernetesClientConfiguration.BuildDefaultConfig();
                return new Kubernetes(config);
            });

            services.TryAddSingleton(sp =>
            {
                var config = sp.GetRequiredService<OperatorConfiguration>();
                var leaderElection = builder.LeaderElection;

                // Set default lease name and namespace if not already set
                if (string.IsNullOrEmpty(leaderElection.LeaseName))
                {
                    leaderElection.LeaseName = $"{config.OperatorName}-leader-election";
                }
                if (string.IsNullOrEmpty(leaderElection.LeaseNamespace))
                {
                    leaderElection.LeaseNamespace = config.Namespace;
                }

                return leaderElection;
            });
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
