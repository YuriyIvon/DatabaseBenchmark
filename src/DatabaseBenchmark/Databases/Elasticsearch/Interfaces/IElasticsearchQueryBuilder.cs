using Elastic.Clients.Elasticsearch;

namespace DatabaseBenchmark.Databases.Elasticsearch.Interfaces
{
    public interface IElasticsearchQueryBuilder
    {
        SearchRequest Build();
    }
}
