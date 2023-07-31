using DatabaseBenchmark.Databases.DynamoDb.Interfaces;

namespace DatabaseBenchmark.Databases.DynamoDb
{
    public class DynamoDbDummyMetricsReporter : IDynamoDbMetricsReporter
    {
        public void IncrementRowCount(long count)
        {
        }
    }
}
