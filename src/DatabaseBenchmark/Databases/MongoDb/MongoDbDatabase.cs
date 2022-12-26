﻿using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.Databases.Model;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Diagnostics;

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

        public ImportResult ImportData(Table table, IDataSource source, int batchSize)
        {
            if (batchSize <= 0)
            {
                batchSize = DefaultImportBatchSize;
            }

            var database = GetDatabase();
            var collection = database.GetCollection<BsonDocument>(table.Name);
            var progressReporter = new ImportProgressReporter(_environment);
            var options = _optionsProvider.GetOptions<MongoDbImportOptions>();
            var insertBuilder = new MongoDbInsertBuilder(table, source) { BatchSize = batchSize };
            var insertExecutor = new MongoDbInsertExecutor(collection, insertBuilder);

            double totalRequestCharge = 0;
            var stopwatch = Stopwatch.StartNew();

            //TODO: dispose correctly
            var preparedInsert = insertExecutor.Prepare();
            while (preparedInsert != null)
            {
                var rowsInserted = preparedInsert.Execute();
                progressReporter.Increment(rowsInserted);

                if (options.CollectCosmosDbRequestUnits)
                {
                    totalRequestCharge += preparedInsert.CustomMetrics[MongoDbConstants.RequestUnitsMetric];
                }

                preparedInsert = insertExecutor.Prepare();
            }

            stopwatch.Stop();

            var rowCount = collection.CountDocuments(new BsonDocument());
            var importResult = new ImportResult(rowCount, stopwatch.ElapsedMilliseconds);

            if (options.CollectCosmosDbRequestUnits)
            {
                importResult.AddMetric(Metrics.TotalRequestCharge, totalRequestCharge);
            }

            return importResult;
        }

        public IQueryExecutorFactory CreateQueryExecutorFactory(Table table, Query query) =>
            new MongoDbQueryExecutorFactory(_connectionString, table, query, _environment, _optionsProvider);

        public IQueryExecutorFactory CreateRawQueryExecutorFactory(RawQuery query) =>
            new MongoDbRawQueryExecutorFactory(_connectionString, query, _environment, _optionsProvider);

        public IQueryExecutorFactory CreateInsertExecutorFactory(Table table, IDataSource source) =>
            new MongoDbInsertExecutorFactory(_connectionString, table, source);

        private IMongoDatabase GetDatabase()
        {
            var client = new MongoClient(_connectionString);
            var databaseName = MongoUrl.Create(_connectionString).DatabaseName;
            return client.GetDatabase(databaseName);
        }
    }
}
