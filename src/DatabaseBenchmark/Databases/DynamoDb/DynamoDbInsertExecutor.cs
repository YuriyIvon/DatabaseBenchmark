using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.DynamoDb.Interfaces;
using DatabaseBenchmark.Model;
using System.Text.Json;

namespace DatabaseBenchmark.Databases.DynamoDb
{
    public sealed class DynamoDbInsertExecutor : IQueryExecutor
    {
        private readonly AmazonDynamoDBClient _client;
        private readonly Table _table;
        private readonly IDynamoDbInsertBuilder _insertBuilder;
        private readonly IDynamoDbMetricsReporter _metricsReporter;
        private readonly IExecutionEnvironment _environment;

        public DynamoDbInsertExecutor(
            AmazonDynamoDBClient client,
            Table table,
            IDynamoDbInsertBuilder insertBuilder,
            IDynamoDbMetricsReporter metricsReporter,
            IExecutionEnvironment environment)
        {
            _client = client;
            _table = table;
            _insertBuilder = insertBuilder;
            _metricsReporter = metricsReporter;
            _environment = environment;
        }

        public IPreparedQuery Prepare()
        {
            var items = _insertBuilder.Build();

            TraceItems(items);

            var itemRequests = items.Select(i =>
                new WriteRequest
                {
                    PutRequest = new PutRequest { Item = i }
                })
                .ToList();

            var batchRequest = new BatchWriteItemRequest
            {
                RequestItems = new Dictionary<string, List<WriteRequest>>
                {
                    [_table.Name] = itemRequests
                },
                ReturnConsumedCapacity = ReturnConsumedCapacity.TOTAL
            };

            return new DynamoDbPreparedInsert(_client, batchRequest, _metricsReporter);
        }

        public IPreparedQuery Prepare(ITransaction transaction) => Prepare();

        public void Dispose() => _client?.Dispose();

        private void TraceItems(IEnumerable<IDictionary<string, AttributeValue>> items)
        {
            if (_environment.TraceQueries)
            {
                foreach (var item in items)
                {
                    _environment.WriteLine(JsonSerializer.Serialize(item));
                }

                _environment.WriteLine(string.Empty);
            }
        }
    }
}
