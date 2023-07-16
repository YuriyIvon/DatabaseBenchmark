using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DatabaseBenchmark.Databases.Common.Interfaces;

namespace DatabaseBenchmark.Databases.DynamoDb
{
    public sealed class DynamoDbPreparedInsert : IPreparedQuery
    {
        private readonly AmazonDynamoDBClient _client;
        private readonly BatchWriteItemRequest _batchRequest;

        private double _capacityUnits;

        public IDictionary<string, double> CustomMetrics =>
            new Dictionary<string, double> { [DynamoDbConstants.CapacityUnitsMetric] = _capacityUnits };

        public IQueryResults Results => null;

        public DynamoDbPreparedInsert(
            AmazonDynamoDBClient client,
            BatchWriteItemRequest batchRequest)
        {
            _client = client;
            _batchRequest = batchRequest;
        }

        public int Execute()
        {
            var response = _client.BatchWriteItemAsync(_batchRequest).Result;

            //TODO: handle possible retries

            _capacityUnits = response.ConsumedCapacity.Sum(cc => cc.CapacityUnits);

            return _batchRequest.RequestItems.Count;
        }

        public void Dispose()
        {
        }
    }
}
