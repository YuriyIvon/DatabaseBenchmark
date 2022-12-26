using Nest;

namespace DatabaseBenchmark.Databases.Elasticsearch.Interfaces
{
    public interface IElasticsearchQueryBuilder
    {
        SearchRequest Build();
    }
}
