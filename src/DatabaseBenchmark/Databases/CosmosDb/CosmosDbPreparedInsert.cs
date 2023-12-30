using DatabaseBenchmark.Databases.Common.Interfaces;
using Microsoft.Azure.Cosmos;

namespace DatabaseBenchmark.Databases.CosmosDb
{
    public sealed class CosmosDbPreparedInsert : IPreparedQuery
    {
        private readonly Container _container;
        private readonly IEnumerable<IDictionary<string, object>> _items;
        private readonly string _partitionKeyName;

        private double _requestCharge;

        public IDictionary<string, double> CustomMetrics =>
            new Dictionary<string, double> { [CosmosDbConstants.RequestUnitsMetric] = _requestCharge };

        public IQueryResults Results => null;

        public CosmosDbPreparedInsert(
            Container container,
            IEnumerable<IDictionary<string, object>> items,
            string partitionKeyName)
        {
            _container = container;
            _items = items;
            _partitionKeyName = partitionKeyName;
        }

        public int Execute()
        {
            var partitionKeyValue = _items.First()[_partitionKeyName];
            var partitionKey = CreatePartitionKey(partitionKeyValue);

            if (_items.Count() == 1)
            {
                var result = _container.CreateItemAsync(_items.First(), partitionKey).Result;
                _requestCharge = result.RequestCharge;
            }
            else
            {
                var transactionalBatch = _container.CreateTransactionalBatch(partitionKey);
                foreach (var item in _items)
                {
                    transactionalBatch.CreateItem(item);
                }

                var result = transactionalBatch.ExecuteAsync().Result;
                _requestCharge = result.RequestCharge;
            }

            return _items.Count();
        }

        public void Dispose()
        {
        }

        private static PartitionKey CreatePartitionKey(object key) =>
            key switch
            {
                bool boolKey => new PartitionKey(boolKey),
                int intKey => new PartitionKey(intKey),
                double doubleKey => new PartitionKey(doubleKey),
                _ => new PartitionKey(key.ToString())
            };
    }
}
