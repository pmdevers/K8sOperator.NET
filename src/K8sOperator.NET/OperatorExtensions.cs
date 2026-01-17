using k8s;
using K8sOperator.NET;
using K8sOperator.NET.Builder;
using K8sOperator.NET.Commands;
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
                ds.Add<GenerateLaunchSettingsCommand>();

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

            services.TryAddSingleton((sp) =>
            {
                var config = builder?.Configuration
                    ?? KubernetesClientConfiguration.BuildDefaultConfig();
                return new Kubernetes(config);
            });
            services.TryAddSingleton<OperatorService>();

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
    public KubernetesClientConfiguration? Configuration { get; set; }
}
