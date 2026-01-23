namespace K8sOperator.NET.Metadata;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AdditionalPrinterColumnAttribute : Attribute
{
    public required string Name { get; set; }
    public required string Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public required string Path { get; set; }
    public int Priority { get; set; } = 0;
}


