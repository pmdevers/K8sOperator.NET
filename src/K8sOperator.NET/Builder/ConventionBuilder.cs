namespace K8sOperator.NET.Builder;

/// <summary>
/// Desribes a Operator Convention Builder
/// </summary>
public interface IConventionBuilder<out T>
{
    /// <summary>
    /// Add Convention
    /// </summary>
    /// <param name="convention"></param>
    void Add(Action<T> convention);

    /// <summary>
    /// Add Finally Convention
    /// </summary>
    /// <param name="convention"></param>
    void Finally(Action<T> convention);
}

internal class ConventionBuilder<T>(
    ICollection<Action<T>> conventions,
    ICollection<Action<T>> finallyConventions) : IConventionBuilder<T>
{
    public void Add(Action<T> convention)
    {
        conventions.Add(convention);
    }

    public void Finally(Action<T> convention)
    {
        finallyConventions.Add(convention);
    }
}
