namespace K8sOperator.NET.Builder;

/// <summary>
/// Desribes a Operator Convention Builder
/// </summary>
public interface IOperatorConventionBuilder
{
    /// <summary>
    /// Add Convention
    /// </summary>
    /// <param name="convention"></param>
    void Add(Action<IOperatorBuilder> convention);

    /// <summary>
    /// Add Finally Convention
    /// </summary>
    /// <param name="convention"></param>
    void Finally(Action<IOperatorBuilder> convention);
}


internal class OperatorConventionBuilder(
    ICollection<Action<IOperatorBuilder>> conventions,
    ICollection<Action<IOperatorBuilder>> finallyConventions) : IOperatorConventionBuilder
{
    public void Add(Action<IOperatorBuilder> convention)
    {
        conventions.Add(convention);
    }

    public void Finally(Action<IOperatorBuilder> convention)
    {
        finallyConventions.Add(convention);
    }
}
