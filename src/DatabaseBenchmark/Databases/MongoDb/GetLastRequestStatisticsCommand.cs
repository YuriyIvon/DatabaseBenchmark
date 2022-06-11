using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace DatabaseBenchmark.Databases.MongoDb
{
    class GetLastRequestStatisticsCommand : Command<Dictionary<string, object>>
    {
        public override RenderedCommand<Dictionary<string, object>> Render(IBsonSerializerRegistry serializerRegistry)
        {
            return new RenderedCommand<Dictionary<string, object>>(new BsonDocument("getLastRequestStatistics", 1), 
                serializerRegistry.GetSerializer<Dictionary<string, object>>());
        }
    }
}
