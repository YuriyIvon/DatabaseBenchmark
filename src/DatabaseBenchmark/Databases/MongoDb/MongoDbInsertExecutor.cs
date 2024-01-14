using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.MongoDb.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DatabaseBenchmark.Databases.MongoDb
{
    public sealed class MongoDbInsertExecutor : IQueryExecutor
    {
        private readonly IMongoCollection<BsonDocument> _collection;
        private readonly IMongoDbInsertBuilder _insertBuilder;
        private readonly MongoDbInsertOptions _options;
        private readonly IExecutionEnvironment _environment;

        public MongoDbInsertExecutor(
            IMongoCollection<BsonDocument> collection,
            IMongoDbInsertBuilder insertBuilder,
            IOptionsProvider optionsProvider,
            IExecutionEnvironment environment)
        {
            _collection = collection;
            _insertBuilder = insertBuilder;

            _options = optionsProvider.GetOptions<MongoDbInsertOptions>();
            _environment = environment;
        }

        public IPreparedQuery Prepare()
        {
            var documents = _insertBuilder.Build();

            TraceDocuments(documents);

            return new MongoDbPreparedInsert(_collection, documents, _options);
        }

        public IPreparedQuery Prepare(ITransaction transaction) => Prepare();

        public void Dispose()
        {
        }

        private void TraceDocuments(IEnumerable<BsonDocument> documents)
        {
            if (_environment.TraceQueries)
            {
                foreach (var document in documents)
                {
                    _environment.WriteLine(document.ToString());
                }

                _environment.WriteLine(string.Empty);
            }
        }
    }
}
