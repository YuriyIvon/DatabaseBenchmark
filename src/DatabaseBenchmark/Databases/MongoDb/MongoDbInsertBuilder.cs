using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.MongoDb.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using MongoDB.Bson;

namespace DatabaseBenchmark.Databases.MongoDb
{
    public class MongoDbInsertBuilder : IMongoDbInsertBuilder
    {
        private readonly Table _table;
        private readonly IDataSource _source;

        public int BatchSize { get; set; } = 1;

        public MongoDbInsertBuilder(Table table, IDataSource source)
        {
            _table = table;
            _source = source;
        }

        public IEnumerable<BsonDocument> Build()
        {
            var documents = new List<BsonDocument>();

            for (int i = 0; i < BatchSize && _source.Read(); i++)
            {
                var document = _table.Columns
                .Where(c => !c.DatabaseGenerated)
                    .ToDictionary(
                        c => c.Name,
                        c => _source.GetValue(c.GetNativeType(), c.Name));

                documents.Add(new BsonDocument(document));
            }

            return documents;
        }
    }
}
