using K8sOperator.NET.Builder;
using Microsoft.Extensions.Hosting;

namespace K8sOperator.NET;

internal class OperatorService(IOperatorBuilder builder) : IHostedService
{
}
