using DatabaseBenchmark.Databases.Interfaces;
using Microsoft.Azure.Cosmos;

namespace DatabaseBenchmark.Databases.CosmosDb
{
    public class CosmosDbPreparedQuery : IPreparedQuery
    {
        private readonly Container _container;
        private readonly QueryDefinition _queryDefinition;
        private readonly CosmosDbQueryOptions _options;

        private FeedIterator<Dictionary<string, object>> _iterator;
        private Dictionary<string, object>[] _responseItems;
        private int _responseItemIndex = 0;
        private double _requestCharge = 0;

        public IEnumerable<string> ColumnNames => _responseItems[_responseItemIndex].Keys;

        public IDictionary<string, double> CustomMetrics =>
            new Dictionary<string, double> { [CosmosDbConstants.RequestUnitsMetric] = _requestCharge };

        public CosmosDbPreparedQuery(Container container, QueryDefinition queryDefinition, CosmosDbQueryOptions options)
        {
            _container = container;
            _queryDefinition = queryDefinition;
            _options = options;
        }

        public object GetValue(string columnName) => _responseItems[_responseItemIndex][columnName];

        public void Execute()
        {
            _iterator = _container.GetItemQueryIterator<Dictionary<string, object>>(
                _queryDefinition,
                requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(CosmosDbConstants.DummyPartitionKeyValue),
                    MaxItemCount = _options.BatchSize
                });
        }

        public bool Read()
        {
            if (_responseItems != null && _responseItemIndex < _responseItems.Length)
            {
                _responseItemIndex++;
            }

            if (_responseItems == null ||
                (_responseItemIndex >= _responseItems.Length && _iterator.HasMoreResults))
            {
                var response = _iterator.ReadNextAsync().Result;
                _responseItems = response.ToArray();
                _responseItemIndex = 0;
                _requestCharge += response.RequestCharge;

                return _responseItems.Any();
            }

            return _responseItemIndex < _responseItems.Length;
        }

        public void Dispose()
        {
            if (_iterator != null)
            {
                _iterator.Dispose();
            }
        }
    }
}