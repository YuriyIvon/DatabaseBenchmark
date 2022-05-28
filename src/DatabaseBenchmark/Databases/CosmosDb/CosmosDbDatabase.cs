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

        public void CreateTable(Table table)
        {
            if (table.Columns.Any(c => c.DatabaseGenerated))
            {
                throw new InputArgumentException("Cosmos DB doesn't support database-generated columns");
            }

            using var client = new CosmosClient(_connectionString);
            var database = client.GetDatabase(_databaseName);
            database.CreateContainerIfNotExistsAsync(table.Name, "/" + CosmosDbConstants.DummyPartitionKeyName).Wait();             
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

            var stopwatch = Stopwatch.StartNew();
            var progressReporter = new ImportProgressReporter(_environment);

            var batch = container.CreateTransactionalBatch(new PartitionKey(CosmosDbConstants.DummyPartitionKeyValue));
            int currentBatchSize = 0;
            int index = 0;

            while (source.Read())
            {
                var item = table.Columns.ToDictionary(
                    x => x.Name,
                    x => source.GetValue(x.Name));

                item.Add("id", Guid.NewGuid().ToString("N"));
                item.Add(CosmosDbConstants.DummyPartitionKeyName, CosmosDbConstants.DummyPartitionKeyValue);

                batch.CreateItem(item);
                currentBatchSize++;
                index++;

                if (currentBatchSize >= batchSize)
                {
                    var result = batch.ExecuteAsync().Result;
                    progressReporter.Increment(currentBatchSize);
                    currentBatchSize = 0;
                    batch = container.CreateTransactionalBatch(new PartitionKey(CosmosDbConstants.DummyPartitionKeyValue));
                }
            }

            if (currentBatchSize > 0)
            {
                batch.ExecuteAsync().Wait();
            }

            stopwatch.Stop();

            return new ImportResult
            {
                Count = container.Count(),
                Duration = stopwatch.ElapsedMilliseconds
            };
        }

        public IQueryExecutorFactory CreateQueryExecutorFactory(Table table, Query query) =>
            new CosmosDbQueryExecutorFactory(_connectionString, _databaseName, table, query, _environment, _optionsProvider);

        public IQueryExecutorFactory CreateRawQueryExecutorFactory(RawQuery query) =>
            new CosmosDbRawQueryExecutorFactory(_connectionString, _databaseName, query, _environment, _optionsProvider);

        private static (string accountConnectionString, string databaseName) ParseConnectionString(string fullConnectionString)
        {
            var parts = fullConnectionString.Split(';');
            var databasePart = parts.First(p => p.StartsWith(DatabaseConnectionStringProperty));
            var databaseName = databasePart.Split('=', StringSplitOptions.TrimEntries).Last();
            return (string.Join(";", parts.Where(p => !p.StartsWith(DatabaseConnectionStringProperty))), databaseName);
        }
    }
}
