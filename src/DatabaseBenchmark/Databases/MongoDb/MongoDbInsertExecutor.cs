using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.Databases.MongoDb.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DatabaseBenchmark.Databases.MongoDb
{
    public sealed class MongoDbInsertExecutor : IQueryExecutor
    {
        private readonly IMongoCollection<BsonDocument> _collection;
        private readonly IMongoDbInsertBuilder _insertBuilder;

        public MongoDbInsertExecutor(
            IMongoCollection<BsonDocument> collection,
            IMongoDbInsertBuilder insertBuilder)
        {
            _collection = collection;
            _insertBuilder = insertBuilder;
        }

        public IPreparedQuery Prepare()
        {
            var documents = _insertBuilder.Build();
            return documents.Any()
                ? new MongoDbPreparedInsert(_collection, documents)
                : null;
        }

        public IPreparedQuery Prepare(ITransaction transaction) => Prepare();

        public void Dispose()
        {
        }
    }
}
