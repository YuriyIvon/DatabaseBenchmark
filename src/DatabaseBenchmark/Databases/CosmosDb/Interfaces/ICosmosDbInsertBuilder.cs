namespace DatabaseBenchmark.Databases.CosmosDb.Interfaces
{
    public interface ICosmosDbInsertBuilder
    {
        int BatchSize { get; }

        string PartitionKeyName { get; }

        IEnumerable<IDictionary<string, object>> Build();
    }
}
