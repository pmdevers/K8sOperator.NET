using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace K8sOperator.NET.Builder;

internal sealed class LoggingBuilder(IServiceCollection services) : ILoggingBuilder
{
    public IServiceCollection Services { get; } = services;
}
