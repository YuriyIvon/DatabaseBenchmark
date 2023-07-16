using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Databases.DynamoDb
{
    public class DynamoDbDataMetricsProvider : IDataMetricsProvider
    {
        private readonly AmazonDynamoDBClient _client;
        private readonly Table _table;

        public long GetRowCount()
        {
            var response = _client.DescribeTableAsync(
                new DescribeTableRequest { TableName = _table.Name }).Result;

            //TODO: find a different way since it doesn't return the actual number
            return response.Table.ItemCount;
        }

        public IDictionary<string, double> GetMetrics() => null;

        public DynamoDbDataMetricsProvider(AmazonDynamoDBClient client, Table table)
        {
            _client = client;
            _table = table;
        }
    }
}
