namespace K8sOperator.NET.Builder;

/// <summary>
/// 
/// </summary>
public interface IControllerConventionBuilder : IConventionBuilder<IControllerBuilder> { }

internal class ControllerConventionBuilder(
    ICollection<Action<IControllerBuilder>> conventions,
    ICollection<Action<IControllerBuilder>> finallyConventions) 
: ConventionBuilder<IControllerBuilder>(conventions, finallyConventions), IControllerConventionBuilder
{
}
