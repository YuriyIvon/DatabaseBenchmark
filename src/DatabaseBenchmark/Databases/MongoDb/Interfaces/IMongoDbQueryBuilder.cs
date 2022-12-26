using MongoDB.Bson;

namespace DatabaseBenchmark.Databases.MongoDb.Interfaces
{
    public interface IMongoDbQueryBuilder
    {
        IEnumerable<BsonDocument> Build();
    }
}
