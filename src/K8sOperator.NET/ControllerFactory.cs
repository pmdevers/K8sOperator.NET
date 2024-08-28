using K8sOperator.NET.Builder;

namespace K8sOperator.NET;
internal static class ControllerFactory
{
    public static IController Create(ControllerFactoryOptions options)
    {

    }
}

internal class ControllerFactoryOptions
{
    public required IServiceProvider ServiceProvider { get; set; }
    public required IOperatorBuilder Builder { get; set; }
}
