using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Interfaces;
using Nest;
using RawQuery = DatabaseBenchmark.Model.RawQuery;

namespace DatabaseBenchmark.Databases.Elasticsearch
{
    public class ElasticsearchRawQueryExecutorFactory : IQueryExecutorFactory
    {
        private readonly Func<ElasticClient> _createClient;
        private readonly RawQuery _query;
        private readonly IRandomGenerator _randomGenerator;

        public ElasticsearchRawQueryExecutorFactory(
            Func<ElasticClient> createClient,
            RawQuery query)
        {
            _createClient = createClient;
            _query = query;
            _randomGenerator = new RandomGenerator();
        }

        public IQueryExecutor Create()
        {
            var client = _createClient();
            var columnPropertiesProvider = new RawQueryColumnPropertiesProvider(_query);
            var distinctValuesProvider = new ElasticsearchDistinctValuesProvider(client);
            var randomValueProvider = new RandomValueProvider(_randomGenerator, columnPropertiesProvider, distinctValuesProvider);
            var queryBuilder = new ElasticsearchRawQueryBuilder(_query, client.RequestResponseSerializer, randomValueProvider);

            return new ElasticsearchQueryExecutor(client, queryBuilder);
        }
    }
}
