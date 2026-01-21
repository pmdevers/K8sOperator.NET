namespace K8sOperator.NET.Metadata;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AdditionalPrinterColumnAttribute : Attribute
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }
    public string Path { get; set; }
    public int Priority { get; set; }
}


