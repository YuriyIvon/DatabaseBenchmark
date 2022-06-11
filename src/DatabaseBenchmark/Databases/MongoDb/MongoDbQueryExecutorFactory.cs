using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.Model;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DatabaseBenchmark.Databases.MongoDb
{
    public class MongoDbQueryExecutorFactory : IQueryExecutorFactory
    {
        private readonly string _connectionString;
        private readonly Table _table;
        private readonly Query _query;
        private readonly IExecutionEnvironment _environment;
        private readonly IOptionsProvider _optionsProvider;
        private readonly IRandomGenerator _randomGenerator;

        public MongoDbQueryExecutorFactory(
            string connectionString,
            Table table,
            Query query,
            IExecutionEnvironment environment,
            IOptionsProvider optionsProvider)
        {
            _connectionString = connectionString;
            _table = table;
            _query = query;
            _environment = environment;
            _randomGenerator = new RandomGenerator();
            _optionsProvider = optionsProvider;
        }

        public IQueryExecutor Create()
        {
            var client = new MongoClient(_connectionString);
            var databaseName = MongoUrl.Create(_connectionString).DatabaseName;
            var database = client.GetDatabase(databaseName);
            var collection = database.GetCollection<BsonDocument>(_table.Name);
            var columnPropertiesProvider = new TableColumnPropertiesProvider(_table);
            var distinctValuesProvider = new MongoDbDistinctValuesProvider(database, _environment);
            var randomValueProvider = new RandomValueProvider(_randomGenerator, columnPropertiesProvider, distinctValuesProvider);
            var queryBuilder = new MongoDbQueryBuilder(_table, _query, randomValueProvider);

            return new MongoDbQueryExecutor(collection, queryBuilder, _environment, _optionsProvider);
        }
    }
}
