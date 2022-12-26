using MongoDB.Bson;

namespace DatabaseBenchmark.Databases.MongoDb.Interfaces
{
    public interface IMongoDbInsertBuilder
    {
        int BatchSize { get; }

        IEnumerable<BsonDocument> Build();
    }
}
