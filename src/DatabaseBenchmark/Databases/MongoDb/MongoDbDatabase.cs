﻿using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.Databases.Model;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Data;
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

        public void CreateTable(Table table)
        {
            //TODO: allow _id field to be marked as database-generated
            if (table.Columns.Any(c => c.DatabaseGenerated))
            {
                _environment.WriteLine("WARNING: MongoDB doesn't support database-generated columns");
            }

            var database = GetDatabase();
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

            var buffer = new List<BsonDocument>();

            var stopwatch = Stopwatch.StartNew();
            var progressReporter = new ImportProgressReporter(_environment);

            while (source.Read())
            {
                var document = table.Columns
                    .Where(c => !c.DatabaseGenerated)
                    .ToDictionary(
                        x => x.Name,
                        x => source.GetValue(x.Name));

                buffer.Add(new BsonDocument(document));

                if (buffer.Count >= batchSize)
                {
                    collection.InsertMany(buffer,
                        new InsertManyOptions
                        {
                            IsOrdered = false
                        });

                    progressReporter.Increment(buffer.Count);
                    buffer.Clear();
                }
            }

            if (buffer.Count > 0)
            {
                collection.InsertMany(buffer);
            }

            stopwatch.Stop();

            return new ImportResult
            {
                Count = collection.CountDocuments(new BsonDocument()),
                Duration = stopwatch.ElapsedMilliseconds,
                TotalStorageBytes = GetTotalStorageSize(database, table.Name)
            };
        }

        public IQueryExecutorFactory CreateQueryExecutorFactory(Table table, Query query) =>
            new MongoDbQueryExecutorFactory(_connectionString, table, query, _environment, _optionsProvider);

        public IQueryExecutorFactory CreateRawQueryExecutorFactory(RawQuery query) =>
            new MongoDbRawQueryExecutorFactory(_connectionString, query, _environment, _optionsProvider);

        private IMongoDatabase GetDatabase()
        {
            var client = new MongoClient(_connectionString);
            var databaseName = MongoUrl.Create(_connectionString).DatabaseName;
            return client.GetDatabase(databaseName);
        }

        private int? GetTotalStorageSize(IMongoDatabase database, string collectionName)
        {
            try
            {
                var collectionStats = database.RunCommand<BsonDocument>(new BsonDocument("collstats", collectionName));
                return (int)collectionStats["totalSize"];
            }
            catch
            {
                _environment.WriteLine("Can't retrieve total storage size");
            }

            return null;
        }
    }
}
