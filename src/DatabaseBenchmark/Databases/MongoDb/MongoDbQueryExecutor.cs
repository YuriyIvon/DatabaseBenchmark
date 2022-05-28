using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DatabaseBenchmark.Databases.MongoDb
{
    public class MongoDbQueryExecutor : IQueryExecutor
    {
        private readonly IMongoCollection<BsonDocument> _collection;
        private readonly IMongoDbQueryBuilder _queryBuilder;
        private readonly IExecutionEnvironment _environment;

        public MongoDbQueryExecutor(
            IMongoCollection<BsonDocument> collection,
            IMongoDbQueryBuilder queryBuilder,
            IExecutionEnvironment environment)
        {
            _collection = collection;
            _queryBuilder = queryBuilder;
            _environment = environment;
        }

        public IPreparedQuery Prepare()
        {
            var request = _queryBuilder.Build();

            if (_environment.TraceQueries)
            {
                _environment.WriteLine(new BsonArray(request).ToString());
            }

            return new MongoDbPreparedQuery(_collection, request);
        }

        public void Dispose()
        {
        }
    }
}
