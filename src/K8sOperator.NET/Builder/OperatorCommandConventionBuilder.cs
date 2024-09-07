
namespace K8sOperator.NET.Builder;

/// <summary>
/// 
/// </summary>
public interface IOperatorCommandConventionBuilder : IConventionBuilder<IOperatorCommandBuilder> { }

internal class OperatorCommandConventionBuilder(
    ICollection<Action<IOperatorCommandBuilder>> conventions,
    ICollection<Action<IOperatorCommandBuilder>> finallyConventions)
: ConventionBuilder<IOperatorCommandBuilder>(conventions, finallyConventions), IOperatorCommandConventionBuilder
{
}
