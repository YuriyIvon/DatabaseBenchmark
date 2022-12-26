using DatabaseBenchmark.Databases.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DatabaseBenchmark.Databases.MongoDb
{
    public sealed class MongoDbPreparedQuery : IPreparedQuery
    {
        private const string RequestUnitsMetric = "RU";

        private readonly IMongoCollection<BsonDocument> _collection;
        private readonly IEnumerable<BsonDocument> _request;
        private readonly MongoDbQueryOptions _options;

        private MongoDbQueryResults _results;

        public IDictionary<string, double> CustomMetrics =>
            _options.CollectCosmosDbRequestUnits 
                ? new Dictionary<string, double> { [RequestUnitsMetric] = _results.RequestCharge } 
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

        public void Execute()
        {
            var cursor = _collection.Aggregate(PipelineDefinition<BsonDocument, BsonDocument>.Create(_request),
                new AggregateOptions
                {
                    BatchSize = _options.BatchSize
                });

            _results = new MongoDbQueryResults(_collection, cursor, _options);
        }

        public void Dispose()
        {
            _results?.Dispose();
        }
    }
}
