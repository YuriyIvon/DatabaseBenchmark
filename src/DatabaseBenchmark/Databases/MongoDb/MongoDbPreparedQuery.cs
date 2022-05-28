using DatabaseBenchmark.Databases.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DatabaseBenchmark.Databases.MongoDb
{
    public class MongoDbPreparedQuery : IPreparedQuery
    {
        private readonly IMongoCollection<BsonDocument> _collection;
        private readonly IEnumerable<BsonDocument> _request;

        private IAsyncCursor<BsonDocument> _cursor;
        private BsonDocument[] _batchItems;
        private int _batchItemIndex = 0;

        public IEnumerable<string> ColumnNames => _batchItems[_batchItemIndex].Names;

        public IDictionary<string, double> CustomMetrics => null;

        public MongoDbPreparedQuery(
            IMongoCollection<BsonDocument> collection,
            IEnumerable<BsonDocument> request)
        {
            _collection = collection;
            _request = request;
        }

        public void Execute()
        {
            _cursor = _collection.Aggregate(PipelineDefinition<BsonDocument, BsonDocument>.Create(_request));
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
