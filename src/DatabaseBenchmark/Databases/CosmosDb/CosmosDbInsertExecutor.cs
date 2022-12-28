using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.CosmosDb.Interfaces;
using Microsoft.Azure.Cosmos;
using System.Text.Json;

namespace DatabaseBenchmark.Databases.CosmosDb
{
    public sealed class CosmosDbInsertExecutor : IQueryExecutor
    {
        private readonly CosmosClient _client;
        private readonly Container _container;
        private readonly ICosmosDbInsertBuilder _insertBuilder;
        private readonly IExecutionEnvironment _environment;

        public CosmosDbInsertExecutor(
            CosmosClient client,
            Container container,
            ICosmosDbInsertBuilder insertBuilder,
            IExecutionEnvironment environment)
        {
            _client = client;
            _container = container;
            _insertBuilder = insertBuilder;
            _environment = environment;
        }

        public IPreparedQuery Prepare()
        {
            var items = _insertBuilder.Build();

            TraceItems(items);

            return items.Any()
                ? new CosmosDbPreparedInsert(_container, items, _insertBuilder.PartitionKeyName)
                : null;
        }

        public IPreparedQuery Prepare(ITransaction transaction) => Prepare();

        public void Dispose() => _client?.Dispose();

        private void TraceItems(IEnumerable<IDictionary<string, object>> items)
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
