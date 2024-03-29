﻿using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.MongoDb.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DatabaseBenchmark.Databases.MongoDb
{
    public sealed class MongoDbQueryExecutor : IQueryExecutor
    {
        private readonly IMongoCollection<BsonDocument> _collection;
        private readonly IMongoDbQueryBuilder _queryBuilder;
        private readonly IExecutionEnvironment _environment;
        private readonly MongoDbQueryOptions _options;

        public MongoDbQueryExecutor(
            IMongoCollection<BsonDocument> collection,
            IMongoDbQueryBuilder queryBuilder,
            IExecutionEnvironment environment,
            IOptionsProvider optionsProvider)
        {
            _collection = collection;
            _queryBuilder = queryBuilder;
            _environment = environment;

            _options = optionsProvider.GetOptions<MongoDbQueryOptions>();
        }

        public IPreparedQuery Prepare()
        {
            var request = _queryBuilder.Build();

            if (_environment.TraceQueries)
            {
                _environment.WriteLine(new BsonArray(request).ToString());
            }

            return new MongoDbPreparedQuery(_collection, request, _options);
        }

        public IPreparedQuery Prepare(ITransaction transaction) => Prepare();

        public void Dispose()
        {
        }
    }
}
