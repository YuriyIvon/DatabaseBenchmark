using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DatabaseBenchmark.Databases.Common.Interfaces;

namespace DatabaseBenchmark.Databases.DynamoDb
{
    public class DynamoDbPreparedQuery : IPreparedQuery
    {
        private readonly AmazonDynamoDBClient _client;
        private readonly ExecuteStatementRequest _request;

        private DynamoDbQueryResults _results;

        public IDictionary<string, double> CustomMetrics =>
            new Dictionary<string, double> { [DynamoDbConstants.CapacityUnitsMetric] = _results.ConsumedCapacity };

        public IQueryResults Results => _results;

        public DynamoDbPreparedQuery(
            AmazonDynamoDBClient client,
            ExecuteStatementRequest request)
        {
            _client = client;
            _request = request;
        }

        public int Execute()
        {
            _results = new DynamoDbQueryResults(_client, _request);

            return 0;
        }

        public void Dispose()
        {
        }
    }
}
