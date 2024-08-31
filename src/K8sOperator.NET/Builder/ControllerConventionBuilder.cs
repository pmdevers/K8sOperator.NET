namespace K8sOperator.NET.Builder;

internal class ControllerConventionBuilder(
    ICollection<Action<IControllerBuilder>> conventions,
    ICollection<Action<IControllerBuilder>> finallyConventions) : IControllerConventionBuilder
{
    public void Add(Action<IControllerBuilder> convention)
    {
        conventions.Add(convention);
    }

    public void Finally(Action<IControllerBuilder> convention)
    {
        finallyConventions.Add(convention);
    }
}
