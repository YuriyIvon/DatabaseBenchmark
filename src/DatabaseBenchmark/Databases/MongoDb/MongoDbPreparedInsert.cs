using DatabaseBenchmark.Databases.Common.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DatabaseBenchmark.Databases.MongoDb
{
    public sealed class MongoDbPreparedInsert : IPreparedQuery
    {
        private readonly IMongoCollection<BsonDocument> _collection;
        private readonly IEnumerable<BsonDocument> _documents;
        private readonly MongoDbInsertOptions _options;

        private double _requestCharge;

        public IDictionary<string, double> CustomMetrics =>
            _options.CollectCosmosDbRequestUnits
                ? new Dictionary<string, double> { [MongoDbConstants.RequestUnitsMetric] = _requestCharge }
                : null;

        public IQueryResults Results => null;

        public MongoDbPreparedInsert(
            IMongoCollection<BsonDocument> collection,
            IEnumerable<BsonDocument> documents,
            MongoDbInsertOptions options)
        {
            _collection = collection;
            _documents = documents;
            _options = options;
        }

        public int Execute()
        {
            _collection.InsertMany(_documents,
                new InsertManyOptions
                {
                    IsOrdered = false
                });

            if (_options.CollectCosmosDbRequestUnits)
            {
                _requestCharge += _collection.GetLastCommandRequestCharge();
            }

            return _documents.Count();
        }

        public void Dispose()
        {
        }
    }
}
