using DatabaseBenchmark.Databases.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DatabaseBenchmark.Databases.MongoDb
{
    public sealed class MongoDbPreparedInsert : IPreparedQuery
    {
        private readonly IMongoCollection<BsonDocument> _collection;
        private readonly IEnumerable<BsonDocument> _documents;

        public IDictionary<string, double> CustomMetrics => null;

        public IQueryResults Results => null;

        public double RequestCharge { get; private set; }

        public MongoDbPreparedInsert(
            IMongoCollection<BsonDocument> collection,
            IEnumerable<BsonDocument> documents)
        {
            _collection = collection;
            _documents = documents;
        }

        public void Execute()
        {
            _collection.InsertMany(_documents,
                new InsertManyOptions
                {
                    IsOrdered = false
                });

            /*if (options.CollectCosmosDbRequestUnits)
            {
                RequestCharge += _collection.GetLastCommandRequestCharge();
            }*/
        }

        public void Dispose()
        {
        }
    }
}
