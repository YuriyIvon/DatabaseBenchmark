using DatabaseBenchmark.Databases.Common.Interfaces;
using Microsoft.Azure.Cosmos;

namespace DatabaseBenchmark.Databases.CosmosDb
{
    public sealed class CosmosDbPreparedQuery : IPreparedQuery
    {
        private readonly Container _container;
        private readonly QueryDefinition _queryDefinition;
        private readonly CosmosDbQueryOptions _options;

        private CosmosDbQueryResults _results;

        public IDictionary<string, double> CustomMetrics =>
            new Dictionary<string, double> { [CosmosDbConstants.RequestUnitsMetric] = _results.RequestCharge };

        public IQueryResults Results => _results;

        public CosmosDbPreparedQuery(Container container, QueryDefinition queryDefinition, CosmosDbQueryOptions options)
        {
            _container = container;
            _queryDefinition = queryDefinition;
            _options = options;
        }

        public int Execute()
        {
            var iterator = _container.GetItemQueryIterator<Dictionary<string, object>>(
                _queryDefinition,
                requestOptions: new QueryRequestOptions
                {
                    MaxItemCount = _options.BatchSize
                });

            _results = new CosmosDbQueryResults(iterator);

            return 0;
        }

        public void Dispose() => _results?.Dispose();
    }
}