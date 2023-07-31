using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.DynamoDb.Interfaces;

namespace DatabaseBenchmark.Databases.DynamoDb
{
    public sealed class DynamoDbPreparedInsert : IPreparedQuery
    {
        private readonly AmazonDynamoDBClient _client;
        private readonly BatchWriteItemRequest _batchRequest;
        private readonly IDynamoDbMetricsReporter _metricsReporter;

        private double _capacityUnits;

        public IDictionary<string, double> CustomMetrics =>
            new Dictionary<string, double> { [DynamoDbConstants.CapacityUnitsMetric] = _capacityUnits };

        public IQueryResults Results => null;

        public DynamoDbPreparedInsert(
            AmazonDynamoDBClient client,
            BatchWriteItemRequest batchRequest,
            IDynamoDbMetricsReporter metricsReporter)
        {
            _client = client;
            _batchRequest = batchRequest;
            _metricsReporter = metricsReporter;
        }

        public int Execute()
        {
            var response = _client.BatchWriteItemAsync(_batchRequest).Result;

            _capacityUnits = response.ConsumedCapacity.Sum(cc => cc.CapacityUnits);
            var requestCount = _batchRequest.RequestItems.Sum(i => i.Value.Count);
            var unprocessedCount = response.UnprocessedItems?.Sum(i => i.Value.Count) ?? 0;
            var processedCount = requestCount - unprocessedCount;

            _metricsReporter.IncrementRowCount(processedCount);

            return processedCount;
        }

        public void Dispose()
        {
        }
    }
}
