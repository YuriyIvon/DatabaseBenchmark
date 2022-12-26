using DatabaseBenchmark.Databases.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DatabaseBenchmark.Databases.MongoDb
{
    //TODO: request charge
    public sealed class MongoDbPreparedInsert : IPreparedQuery
    {
        private readonly IMongoCollection<BsonDocument> _collection;
        private readonly IEnumerable<BsonDocument> _documents;

        //private double _requestCharge;

        public IDictionary<string, double> CustomMetrics => null;
            /*_options.CollectCosmosDbRequestUnits
                ? new Dictionary<string, double> { [MongoDbConstants.RequestUnitsMetric] = _requestCharge }
                : null;*/

        public IQueryResults Results => null;

        public MongoDbPreparedInsert(
            IMongoCollection<BsonDocument> collection,
            IEnumerable<BsonDocument> documents)
        {
            _collection = collection;
            _documents = documents;
        }

        public int Execute()
        {
            _collection.InsertMany(_documents,
                new InsertManyOptions
                {
                    IsOrdered = false
                });

            return _documents.Count();

            /*if (_options.CollectCosmosDbRequestUnits)
            {
                _requestCharge += _collection.GetLastCommandRequestCharge();
            }*/
        }

        public void Dispose()
        {
        }
    }
}
