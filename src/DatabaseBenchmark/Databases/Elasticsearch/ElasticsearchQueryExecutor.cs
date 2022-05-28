using DatabaseBenchmark.Databases.Interfaces;
using Nest;

namespace DatabaseBenchmark.Databases.Elasticsearch
{
    public class ElasticsearchQueryExecutor : IQueryExecutor
    {
        private readonly ElasticClient _client;
        private readonly IElasticsearchQueryBuilder _queryBuilder;

        public ElasticsearchQueryExecutor(
            ElasticClient client,
            IElasticsearchQueryBuilder queryBuilder)
        {
            _client = client;
            _queryBuilder = queryBuilder;
        }

        public IPreparedQuery Prepare()
        {
            var request = _queryBuilder.Build();

            return new ElasticsearchPreparedQuery(_client, request);
        }

        public void Dispose()
        {
        }
    }
}
