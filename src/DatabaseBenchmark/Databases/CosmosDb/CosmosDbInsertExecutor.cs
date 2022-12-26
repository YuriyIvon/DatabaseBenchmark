using DatabaseBenchmark.Databases.CosmosDb.Interfaces;
using DatabaseBenchmark.Databases.Interfaces;
using Microsoft.Azure.Cosmos;

namespace DatabaseBenchmark.Databases.CosmosDb
{
    public sealed class CosmosDbInsertExecutor : IQueryExecutor
    {
        private readonly CosmosClient _client;
        private readonly Container _container;
        private readonly ICosmosDbInsertBuilder _insertBuilder;

        public CosmosDbInsertExecutor(
            CosmosClient client,
            Container container,
            ICosmosDbInsertBuilder insertBuilder)
        {
            _client = client;
            _container = container;
            _insertBuilder = insertBuilder;
        }

        public IPreparedQuery Prepare()
        {
            var items = _insertBuilder.Build();
            return items.Any()
                ? new CosmosDbPreparedInsert(_container, items, _insertBuilder.PartitionKeyName)
                : null;
        }

        public void Dispose() => _client?.Dispose();
    }
}
