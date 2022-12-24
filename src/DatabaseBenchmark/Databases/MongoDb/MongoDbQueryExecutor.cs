using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DatabaseBenchmark.Databases.MongoDb
{
    public sealed class MongoDbQueryExecutor : IQueryExecutor
    {
        private readonly IMongoCollection<BsonDocument> _collection;
        private readonly IMongoDbQueryBuilder _queryBuilder;
        private readonly IExecutionEnvironment _environment;
        private readonly IOptionsProvider _optionsProvider;

        public MongoDbQueryExecutor(
            IMongoCollection<BsonDocument> collection,
            IMongoDbQueryBuilder queryBuilder,
            IExecutionEnvironment environment,
            IOptionsProvider optionsProvider)
        {
            _collection = collection;
            _queryBuilder = queryBuilder;
            _environment = environment;
            _optionsProvider = optionsProvider;
        }

        public IPreparedQuery Prepare()
        {
            var request = _queryBuilder.Build();

            if (_environment.TraceQueries)
            {
                _environment.WriteLine(new BsonArray(request).ToString());
            }

            var options = _optionsProvider.GetOptions<MongoDbQueryOptions>();
            return new MongoDbPreparedQuery(_collection, request, options);
        }

        public void Dispose()
        {
        }
    }
}
