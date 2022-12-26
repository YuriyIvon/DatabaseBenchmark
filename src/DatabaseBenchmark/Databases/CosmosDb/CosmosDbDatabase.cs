using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.Databases.Model;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using Microsoft.Azure.Cosmos;
using System.Data;
using System.Diagnostics;

namespace DatabaseBenchmark.Databases.CosmosDb
{
    public class CosmosDbDatabase : IDatabase
    {
        private const int DefaultImportBatchSize = 100;

        private const string DatabaseConnectionStringProperty = "Database";

        private readonly string _connectionString;
        private readonly string _databaseName;
        private readonly IExecutionEnvironment _environment;
        private readonly IOptionsProvider _optionsProvider;

        public CosmosDbDatabase(
            string connectionString,
            IExecutionEnvironment environment,
            IOptionsProvider optionsProvider)
        {
            (_connectionString, _databaseName) = ParseConnectionString(connectionString);
            _environment = environment;
            _optionsProvider = optionsProvider;
        }

        public void CreateTable(Table table, bool dropExisting)
        {
            if (table.Columns.Any(c => c.DatabaseGenerated))
            {
                _environment.WriteLine("WARNING: Cosmos DB doesn't support database-generated columns");
            }

            using var client = new CosmosClient(_connectionString);
            var database = client.GetDatabase(_databaseName);

            if (dropExisting)
            {
                try
                {
                    var container = database.GetContainer(table.Name);
                    container.DeleteContainerAsync().Wait();
                }
                catch
                {
                }
            }

            var partitionKeyName = GetPartitionKeyName(table);
            database.CreateContainerIfNotExistsAsync(table.Name, "/" + partitionKeyName).Wait();             
        }

        public ImportResult ImportData(Table table, IDataSource source, int batchSize)
        {
            if (batchSize == 0)
            {
                batchSize = DefaultImportBatchSize;
            }

            using var client = new CosmosClient(_connectionString);
            var database = client.GetDatabase(_databaseName);
            var container = database.GetContainer(table.Name);

            double totalRequestCharge = 0;
            var stopwatch = Stopwatch.StartNew();
            var progressReporter = new ImportProgressReporter(_environment);

            var batches = new Dictionary<object, List<Dictionary<string, object>>>();
            var partitionKeyName = GetPartitionKeyName(table);

            while (source.Read())
            {
                var item = table.Columns
                    .Where(c => !c.DatabaseGenerated)
                    .ToDictionary(
                        c => c.Name,
                        c => source.GetValue(c.GetNativeType(), c.Name));

                item.Add("id", Guid.NewGuid().ToString("N"));

                if (partitionKeyName == CosmosDbConstants.DummyPartitionKeyName)
                {
                    item.Add(partitionKeyName, CosmosDbConstants.DummyPartitionKeyValue);
                }

                var partitionKeyValue = item[partitionKeyName];

                if (!batches.TryGetValue(partitionKeyValue, out var batch))
                {
                    batch = new List<Dictionary<string, object>>();
                    batches.Add(partitionKeyValue, batch);
                }

                batch.Add(item);

                if (batch.Count >= batchSize)
                {
                    var partitionKey = CreatePartitionKey(partitionKeyValue);
                    var transactionalBatch = container.CreateTransactionalBatch(partitionKey);
                    batch.ForEach(i => transactionalBatch.CreateItem(i));
                    var result = transactionalBatch.ExecuteAsync().Result;
                    totalRequestCharge += result.RequestCharge;
                    progressReporter.Increment(batch.Count);
                    batch.Clear();
                }
            }

            foreach (var batchEntry in batches)
            {
                if (batchEntry.Value.Any())
                {
                    var partitionKey = CreatePartitionKey(batchEntry.Key);
                    var transactionalBatch = container.CreateTransactionalBatch(partitionKey);
                    batchEntry.Value.ForEach(i => transactionalBatch.CreateItem(i));
                    var result = transactionalBatch.ExecuteAsync().Result;
                    totalRequestCharge += result.RequestCharge;
                }
            }

            stopwatch.Stop();

            var importResult = new ImportResult(container.Count(), stopwatch.ElapsedMilliseconds);
            importResult.AddMetric(Metrics.TotalRequestCharge, totalRequestCharge);

            return importResult;
        }

        public IQueryExecutorFactory CreateQueryExecutorFactory(Table table, Query query) =>
            new CosmosDbQueryExecutorFactory(_connectionString, _databaseName, table, query, _environment, _optionsProvider);

        public IQueryExecutorFactory CreateRawQueryExecutorFactory(RawQuery query) =>
            new CosmosDbRawQueryExecutorFactory(_connectionString, _databaseName, query, _environment, _optionsProvider);

        public IQueryExecutorFactory CreateInsertExecutorFactory(Table table, IDataSource source) =>
            new CosmosDbInsertExecutorFactory(_connectionString, _databaseName, table, source);

        private static (string accountConnectionString, string databaseName) ParseConnectionString(string fullConnectionString)
        {
            var parts = fullConnectionString.Split(';');
            var databasePart = parts.First(p => p.StartsWith(DatabaseConnectionStringProperty));
            var databaseName = databasePart.Split('=', StringSplitOptions.TrimEntries).Last();
            return (string.Join(";", parts.Where(p => !p.StartsWith(DatabaseConnectionStringProperty))), databaseName);
        }

        private static string GetPartitionKeyName(Table table)
        {
            if (table.Columns.Count(c => c.PartitionKey) > 1)
            {
                throw new InputArgumentException("A table can't have multiple partition keys");
            }

            return table.Columns.FirstOrDefault(c => c.PartitionKey)?.Name ?? CosmosDbConstants.DummyPartitionKeyName;
        }

        private static PartitionKey CreatePartitionKey(object key) =>
            key switch
            {
                bool boolKey => new PartitionKey(boolKey),
                double doubleKey => new PartitionKey(doubleKey),
                _ => new PartitionKey(key.ToString())
            };
    }
}
