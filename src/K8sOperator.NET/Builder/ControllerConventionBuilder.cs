namespace K8sOperator.NET.Builder;

/// <summary>
/// Desribes a Operator Convention Builder
/// </summary>
public interface IControllerConventionBuilder
{
    /// <summary>
    /// Add Convention
    /// </summary>
    /// <param name="convention"></param>
    void Add(Action<IControllerBuilder> convention);

    /// <summary>
    /// Add Finally Convention
    /// </summary>
    /// <param name="convention"></param>
    void Finally(Action<IControllerBuilder> convention);
}


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
