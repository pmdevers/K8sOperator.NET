namespace K8sOperator.NET.Builder;

public class ConventionBuilder<T>(ICollection<Action<T>> conventions)
{
    public void Add(Action<T> convention)
    {
        conventions.Add(convention);
    }
}
