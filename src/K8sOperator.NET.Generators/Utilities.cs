using System.Reflection;

namespace K8sOperator.NET.Helpers;
internal static class Utilities
{
    /// <summary>
    /// Check if a type is nullable.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>True if the type is nullable (i.e. contains "nullable" in its name).</returns>
    public static bool IsNullable(this Type type)
        => type.FullName?.Contains("Nullable") == true;
    /// <summary>
    /// Check if a property is nullable.
    /// </summary>
    /// <param name="prop">The property.</param>
    /// <returns>True if the type is nullable (i.e. contains "nullable" in its name).</returns>
    public static bool IsNullable(this PropertyInfo prop)
        => new NullabilityInfoContext().Create(prop).ReadState == NullabilityState.Nullable ||
           prop.PropertyType.FullName?.Contains("Nullable") == true;
}
