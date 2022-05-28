using MongoDB.Bson;

namespace DatabaseBenchmark.Databases.MongoDb
{
    public interface IMongoDbQueryBuilder
    {
        IEnumerable<BsonDocument> Build();
    }
}
