using K8sOperator.NET;
using K8sOperator.NET.Metadata;
using System.Reflection;
using System.Threading.Tasks;

namespace K8sOperator.NET.Target.Tests;

public class OperatorTargets_Tests
{
    [Test]
    public async Task Operator_Targets_Adds_OperatorName_Attribute()
    {
        // Arrange
        var assembly = System.Reflection.Assembly.GetEntryAssembly() ?? System.Reflection.Assembly.GetExecutingAssembly();

        // Act
        var attribute = assembly.GetCustomAttribute<OperatorNameAttribute>();

        // Assert
        await Assert.That(attribute).IsNotNull();
    }

    [Test]
    public async Task Operator_Targets_Adds_Namespace_Attribute()
    {
        // Arrange
        var assembly = System.Reflection.Assembly.GetEntryAssembly() ?? System.Reflection.Assembly.GetExecutingAssembly();

        // Act
        var attribute = assembly.GetCustomAttribute<NamespaceAttribute>();

        // Assert
        await Assert.That(attribute).IsNotNull();
    }

    [Test]
    public async Task Operator_Targets_Adds_DockerImage_Attribute()
    {
        // Arrange
        var assembly = System.Reflection.Assembly.GetEntryAssembly() ?? System.Reflection.Assembly.GetExecutingAssembly();

        // Act
        var attribute = assembly.GetCustomAttribute<DockerImageAttribute>();

        // Assert
        await Assert.That(attribute).IsNotNull();
    }
}
