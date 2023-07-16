using Amazon.DynamoDBv2.Model;

namespace DatabaseBenchmark.Databases.DynamoDb.Interfaces
{
    public interface IDynamoDbInsertBuilder
    {
        int BatchSize { get; }

        string PartitionKeyName { get; }

        IEnumerable<Dictionary<string, AttributeValue>> Build();
    }
}
