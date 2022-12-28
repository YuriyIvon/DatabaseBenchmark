using DatabaseBenchmark.Databases.Common.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DatabaseBenchmark.Databases.MongoDb
{
    public class MongoDbDataMetricsProvider : IDataMetricsProvider
    {
        private readonly IMongoCollection<BsonDocument> _collection;

        public MongoDbDataMetricsProvider(IMongoCollection<BsonDocument> collection)
        {
            _collection = collection;
        }

        public long GetRowCount() => _collection.CountDocuments(new BsonDocument());
    }
}
