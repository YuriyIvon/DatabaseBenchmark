using Nest;

namespace DatabaseBenchmark.Databases.Elasticsearch
{
    public interface IElasticsearchQueryBuilder
    {
        SearchRequest Build();
    }
}
