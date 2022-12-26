namespace DatabaseBenchmark.Databases.CosmosDb.Interfaces
{
    public interface ICosmosDbInsertBuilder
    {
        int BatchSize { get; }

        IEnumerable<object> Build();
    }
}
