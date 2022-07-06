using MongoDB.Driver;

namespace DatabaseBenchmark.Databases.MongoDb
{
    public static class MongoDbCollectionExtensions
    {
        public static double GetLastCommandRequestCharge<T>(this IMongoCollection<T> collection)
        {
            var stats = collection.Database.RunCommand(new GetLastRequestStatisticsCommand());
            return (double)stats["RequestCharge"];
        }
    }
}
