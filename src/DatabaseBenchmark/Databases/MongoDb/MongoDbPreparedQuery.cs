using DatabaseBenchmark.Databases.Common.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DatabaseBenchmark.Databases.MongoDb
{
    public sealed class MongoDbPreparedQuery : IPreparedQuery
    {
        private readonly IMongoCollection<BsonDocument> _collection;
        private readonly IEnumerable<BsonDocument> _request;
        private readonly MongoDbQueryOptions _options;

        private MongoDbQueryResults _results;

        public IDictionary<string, double> CustomMetrics =>
            _options.CollectCosmosDbRequestUnits 
                ? new Dictionary<string, double> { [MongoDbConstants.RequestUnitsMetric] = _results.RequestCharge } 
                : null;

        public IQueryResults Results => _results;

        public MongoDbPreparedQuery(
            IMongoCollection<BsonDocument> collection,
            IEnumerable<BsonDocument> request,
            MongoDbQueryOptions options)
        {
            _collection = collection;
            _request = request;
            _options = options;
        }

        public int Execute()
        {
            var cursor = _collection.Aggregate(PipelineDefinition<BsonDocument, BsonDocument>.Create(_request),
                new AggregateOptions
                {
                    BatchSize = _options.BatchSize
                });

            _results = new MongoDbQueryResults(_collection, cursor, _options);

            return 0;
        }

        public void Dispose() => _results?.Dispose();
    }
}
