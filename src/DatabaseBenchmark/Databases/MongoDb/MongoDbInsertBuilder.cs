using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.MongoDb.Interfaces;
using DatabaseBenchmark.Model;
using MongoDB.Bson;

namespace DatabaseBenchmark.Databases.MongoDb
{
    public class MongoDbInsertBuilder : IMongoDbInsertBuilder
    {
        private readonly Table _table;
        private readonly IDataSourceReader _sourceReader;
        private readonly InsertBuilderOptions _options;

        public int BatchSize => _options.BatchSize;

        public MongoDbInsertBuilder(
            Table table,
            IDataSourceReader sourceReader,
            InsertBuilderOptions options)
        {
            _table = table;
            _sourceReader = sourceReader;
            _options = options;
        }

        public IEnumerable<BsonDocument> Build()
        {
            var documents = new List<BsonDocument>();

            for (int i = 0; i < BatchSize && _sourceReader.ReadDictionary(_table.Columns, out var document); i++)
            {
                documents.Add(new BsonDocument(document));
            }

            return documents;
        }
    }
}
