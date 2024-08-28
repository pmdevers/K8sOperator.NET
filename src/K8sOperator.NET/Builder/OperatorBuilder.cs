namespace K8sOperator.NET.Builder;

internal class OperatorBuilder(IServiceProvider serviceProvider) : IOperatorBuilder
{
    public IServiceProvider ServiceProvider { get; } = serviceProvider;

    public IOperatorDataSource DataSource { get; set; } = new OperatorDatasource(serviceProvider);

    public List<object> MetaData { get; } = [];
}
