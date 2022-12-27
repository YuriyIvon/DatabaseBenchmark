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

            var progressReporter = new ImportProgressReporter(_environment);
            var sourceReader = new DataSourceReader(source);
            var insertBuilder = new CosmosDbInsertBuilder(table, sourceReader) { BatchSize = batchSize };
            var insertExecutor = new CosmosDbInsertExecutor(client, container, insertBuilder);

            double totalRequestCharge = 0;
            var stopwatch = Stopwatch.StartNew();

            //TODO: dispose correctly
            var preparedInsert = insertExecutor.Prepare();
            while (preparedInsert != null)
            {
                var rowsInserted = preparedInsert.Execute();
                progressReporter.Increment(rowsInserted);
                totalRequestCharge += preparedInsert.CustomMetrics[CosmosDbConstants.RequestUnitsMetric];
                preparedInsert = insertExecutor.Prepare();
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
    }
}
