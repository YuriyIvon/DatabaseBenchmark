using DatabaseBenchmark.Databases.Common.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DatabaseBenchmark.Databases.MongoDb
{
    public sealed class MongoDbQueryResults : IQueryResults, IDisposable
    {
        private readonly IMongoCollection<BsonDocument> _collection;
        private readonly IAsyncCursor<BsonDocument> _cursor;
        private readonly MongoDbQueryOptions _options;

        private BsonDocument[] _batchItems;
        private int _batchItemIndex = 0;

        public IEnumerable<string> ColumnNames => _batchItems[_batchItemIndex].Names;

        public double RequestCharge { get; private set; }

        public MongoDbQueryResults(
            IMongoCollection<BsonDocument> collection,
            IAsyncCursor<BsonDocument> cursor,
            MongoDbQueryOptions options)
        {
            _collection = collection;
            _cursor = cursor;
            _options = options;
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
                        RequestCharge += _collection.GetLastCommandRequestCharge();
                    }

                    return _batchItems.Any();
                }

                return false;
            }

            return _batchItemIndex < _batchItems.Length;
        }

        public void Dispose() => _cursor?.Dispose();

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
                BsonArray bsonArray => bsonArray.Select(ToStandardType).ToArray(),
                BsonNull => null,
                _ => value
            };
    }
}
