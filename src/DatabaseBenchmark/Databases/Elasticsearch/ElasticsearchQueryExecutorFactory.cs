using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.Model;
using Nest;

namespace DatabaseBenchmark.Databases.Elasticsearch
{
    public class ElasticsearchQueryExecutorFactory : IQueryExecutorFactory
    {
        private readonly Func< ElasticClient> _createClient;
        private readonly Table _table;
        private readonly Query _query;
        private readonly IRandomGenerator _randomGenerator;

        public ElasticsearchQueryExecutorFactory(
            Func<ElasticClient> createClient,
            Table table,
            Query query)
        {
            _createClient = createClient;
            _table = table;
            _query = query;
            _randomGenerator = new RandomGenerator();
        }

        public IQueryExecutor Create()
        {
            var client = _createClient();
            var columnPropertiesProvider = new TableColumnPropertiesProvider(_table);
            var distinctValuesProvider = new ElasticsearchDistinctValuesProvider(client);
            var randomValueProvider = new RandomValueProvider(_randomGenerator, columnPropertiesProvider, distinctValuesProvider);
            var queryBuilder = new ElasticsearchQueryBuilder(_table, _query, randomValueProvider);

            return new ElasticsearchQueryExecutor(client, queryBuilder);
        }
    }
}
