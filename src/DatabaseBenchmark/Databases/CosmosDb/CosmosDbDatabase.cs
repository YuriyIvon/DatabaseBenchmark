using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.CosmosDb.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using Microsoft.Azure.Cosmos;
using System.Data;

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
                    //TODO: put a specific exception into catch statement
                }
            }

            var partitionKeyName = table.GetPartitionKeyName();
            database.CreateContainerIfNotExistsAsync(table.Name, "/" + partitionKeyName).Wait();             
        }

        public IDataImporter CreateDataImporter(Table table, IDataSource source, int batchSize) =>
            new DataImporterBuilder(table, source, batchSize, DefaultImportBatchSize)
                .TransactionProvider<NoTransactionProvider>()
                .InsertBuilder<ICosmosDbInsertBuilder, CosmosDbInsertBuilder>()
                .InsertExecutor<CosmosDbInsertExecutor>()
                .DataMetricsProvider<CosmosDbDataMetricsProvider>()
                .ProgressReporter<ImportProgressReporter>()
                .OptionsProvider(_optionsProvider)
                .Environment(_environment)
                .Customize((container, lifestyle) =>
                {
                    container.Register<CosmosClient>(() => new CosmosClient(_connectionString), lifestyle);
                    container.Register<Database>(() => container.GetInstance<CosmosClient>().GetDatabase(_databaseName), lifestyle);
                    container.Register<Container>(() => container.GetInstance<Database>().GetContainer(table.Name), lifestyle);
                })
                .Build();

        public IQueryExecutorFactory CreateQueryExecutorFactory(Table table, Query query) =>
            new CosmosDbQueryExecutorFactory(_connectionString, _databaseName, table, query, _environment, _optionsProvider);

        public IQueryExecutorFactory CreateRawQueryExecutorFactory(RawQuery query) =>
            new CosmosDbRawQueryExecutorFactory(_connectionString, _databaseName, query, _environment, _optionsProvider);

        public IQueryExecutorFactory CreateInsertExecutorFactory(Table table, IDataSource source, int batchSize) =>
            new CosmosDbInsertExecutorFactory(_connectionString, _databaseName, table, source, batchSize, _environment);

        public void ExecuteScript(string script) => throw new InputArgumentException("Custom scripts are not supported for CosmosDB");

        private static (string accountConnectionString, string databaseName) ParseConnectionString(string fullConnectionString)
        {
            var parts = fullConnectionString.Split(';');
            var databasePart = parts.First(p => p.StartsWith(DatabaseConnectionStringProperty));
            var databaseName = databasePart.Split('=', StringSplitOptions.TrimEntries).Last();
            return (string.Join(";", parts.Where(p => !p.StartsWith(DatabaseConnectionStringProperty))), databaseName);
        }
    }
}
