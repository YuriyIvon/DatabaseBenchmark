namespace DatabaseBenchmark.Databases.Elasticsearch.Interfaces
{
    public interface IElasticsearchInsertBuilder
    {
        int BatchSize { get; }

        IEnumerable<object> Build();
    }
}
