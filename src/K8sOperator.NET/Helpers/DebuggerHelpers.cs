namespace K8sOperator.NET.Helpers;

internal static class DebuggerHelpers
{
    public static string GetDebugText(string key, object value)
        => $"'{key}' - {value}";

}
