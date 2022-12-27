using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.Databases.MongoDb.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using MongoDB.Bson;

namespace DatabaseBenchmark.Databases.MongoDb
{
    public class MongoDbInsertBuilder : IMongoDbInsertBuilder
    {
        private readonly Table _table;
        private readonly IDataSourceReader _sourceReader;

        public int BatchSize { get; set; } = 1;

        public MongoDbInsertBuilder(Table table, IDataSourceReader sourceReader)
        {
            _table = table;
            _sourceReader = sourceReader;
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
