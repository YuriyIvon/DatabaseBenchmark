using DatabaseBenchmark.Databases.Elasticsearch.Interfaces;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.Model;
using Nest;

namespace DatabaseBenchmark.Databases.Elasticsearch
{
    public sealed class ElasticsearchInsertExecutor : IQueryExecutor
    {
        private readonly IElasticClient _client;
        private readonly Table _table;
        private readonly IElasticsearchInsertBuilder _insertBuilder;

        public ElasticsearchInsertExecutor(
            IElasticClient client,
            Table table,
            IElasticsearchInsertBuilder insertBuilder)
        {
            _client = client;
            _table = table;
            _insertBuilder = insertBuilder;
        }

        public IPreparedQuery Prepare()
        {
            var documents = _insertBuilder.Build();
            return documents.Any() 
                ? new ElasticsearchPreparedInsert(_client, _table, documents)
                : null;
        }

        public void Dispose()
        {
        }
    }
}
