using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.MongoDb.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DatabaseBenchmark.Databases.MongoDb
{
    public class MongoDbDatabase : IDatabase
    {
        private const int DefaultImportBatchSize = 500;

        private readonly string _connectionString;
        private readonly IExecutionEnvironment _environment;
        private readonly IOptionsProvider _optionsProvider;

        public MongoDbDatabase(
            string connectionString,
            IExecutionEnvironment environment,
            IOptionsProvider optionsProvider)
        {
            _connectionString = connectionString;
            _environment = environment;
            _optionsProvider = optionsProvider;
        }

        public void CreateTable(Table table, bool dropExisting)
        {
            //TODO: allow _id field to be marked as database-generated
            if (table.Columns.Any(c => c.DatabaseGenerated))
            {
                _environment.WriteLine("WARNING: MongoDB doesn't support database-generated columns");
            }

            var database = GetDatabase();

            if (dropExisting)
            {
                var collection = database.GetCollection<BsonDocument>(table.Name);
                if (collection != null)
                {
                    database.DropCollection(table.Name);
                }
            }

            database.CreateCollection(table.Name);
        }

        public IDataImporter CreateDataImporter(Table table, IDataSource source, int batchSize) =>
            new DataImporterBuilder(table, source, batchSize, DefaultImportBatchSize)
                .OptionsProvider(_optionsProvider)
                .Environment(_environment)
                .TransactionProvider<NoTransactionProvider>()
                .InsertBuilder<IMongoDbInsertBuilder, MongoDbInsertBuilder>()
                .InsertExecutor<MongoDbInsertExecutor>()
                .DataMetricsProvider<MongoDbDataMetricsProvider>()
                .ProgressReporter<ImportProgressReporter>()
                .Customize((container, lifestyle) =>
                {
                    container.Register<IMongoDatabase>(GetDatabase, lifestyle);
                    container.Register<IMongoCollection<BsonDocument>>(() =>
                        container.GetInstance<IMongoDatabase>().GetCollection<BsonDocument>(table.Name), lifestyle);
                })
                .Build();

        public IQueryExecutorFactory CreateQueryExecutorFactory(Table table, Query query) =>
            new MongoDbQueryExecutorFactory(_connectionString, table, query, _environment, _optionsProvider);

        public IQueryExecutorFactory CreateRawQueryExecutorFactory(RawQuery query) =>
            new MongoDbRawQueryExecutorFactory(_connectionString, query, _environment, _optionsProvider);

        public IQueryExecutorFactory CreateInsertExecutorFactory(Table table, IDataSource source) =>
            new MongoDbInsertExecutorFactory(_connectionString, table, source, _optionsProvider);

        private IMongoDatabase GetDatabase()
        {
            var client = new MongoClient(_connectionString);
            var databaseName = MongoUrl.Create(_connectionString).DatabaseName;
            return client.GetDatabase(databaseName);
        }
    }
}
