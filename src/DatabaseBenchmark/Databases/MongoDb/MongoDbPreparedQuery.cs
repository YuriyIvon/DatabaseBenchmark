using DatabaseBenchmark.Databases.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DatabaseBenchmark.Databases.MongoDb
{
    public class MongoDbPreparedQuery : IPreparedQuery
    {
        private const string RequestUnitsMetric = "RU";

        private readonly IMongoCollection<BsonDocument> _collection;
        private readonly IEnumerable<BsonDocument> _request;
        private readonly MongoDbQueryOptions _options;

        private IAsyncCursor<BsonDocument> _cursor;
        private BsonDocument[] _batchItems;
        private int _batchItemIndex = 0;
        private double _requestCharge = 0;

        public IEnumerable<string> ColumnNames => _batchItems[_batchItemIndex].Names;

        public IDictionary<string, double> CustomMetrics =>
            _options.CollectCosmosDbRequestUnits 
                ? new Dictionary<string, double> { [RequestUnitsMetric] = _requestCharge } 
                : null;

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
            _cursor = _collection.Aggregate(PipelineDefinition<BsonDocument, BsonDocument>.Create(_request),
                new AggregateOptions
                {
                    BatchSize = _options.BatchSize
                });
        }

        public object GetValue(string columnName) => ToStandardType(_batchItems[_batchItemIndex][columnName]);

        public bool Read()
        {
            if (_batchItems != null && _batchItemIndex < _batchItems.Length)
            {
                _batchItemIndex++;
            }

            if (_batchItems == null || _batchItemIndex >= _batchItems.Length)
            {
                if (_cursor.MoveNext())
                {
                    _batchItems = _cursor.Current.ToArray();
                    _batchItemIndex = 0;

                    if (_options.CollectCosmosDbRequestUnits)
                    {
                        _requestCharge += _collection.GetLastCommandRequestCharge();
                    }

                    return _batchItems.Any();
                }

                return false;
            }

            return _batchItemIndex < _batchItems.Length;
        }

        public void Dispose()
        {
            if (_cursor != null)
            {
                _cursor.Dispose();
            }
        }

        private static object ToStandardType(object value) =>
            value switch
            {
                BsonObjectId bsonObjectId => bsonObjectId.Value.ToString(),
                BsonBoolean bsonBoolean => bsonBoolean.Value,
                BsonInt32 bsonInt32 => bsonInt32.Value,
                BsonInt64 bsonInt64 => bsonInt64.Value,
                BsonDouble bsonDouble => bsonDouble.Value,
                BsonString bsonString => bsonString.Value,
                BsonDateTime bsonDateTime => bsonDateTime.ToNullableUniversalTime(),
                BsonNull => null,
                _ => value
            };
    }
}
