namespace DatabaseBenchmark.Databases.DynamoDb.Interfaces
{
    //Is introduced for a workardound to calculate the number of successfully inserted rows on import
    public interface IDynamoDbMetricsReporter
    {
        void IncrementRowCount(long count);
    }
}
