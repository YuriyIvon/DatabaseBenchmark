using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.Model;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DatabaseBenchmark.Databases.MongoDb
{
    public class MongoDbRawQueryExecutorFactory : IQueryExecutorFactory
    {
        private readonly string _connectionString;
        private readonly RawQuery _query;
        private readonly IExecutionEnvironment _environment;
        private readonly IRandomGenerator _randomGenerator;

        public MongoDbRawQueryExecutorFactory(
            string connectionString,
            RawQuery query,
            IExecutionEnvironment environment)
        {
            _connectionString = connectionString;
            _query = query;
            _environment = environment;
            _randomGenerator = new RandomGenerator();
        }

        public IQueryExecutor Create()
        {
            var client = new MongoClient(_connectionString);
            var databaseName = MongoUrl.Create(_connectionString).DatabaseName;
            var database = client.GetDatabase(databaseName);
            var collection = database.GetCollection<BsonDocument>(_query.TableName);
            var columnPropertiesProvider = new RawQueryColumnPropertiesProvider(_query);
            var distinctValuesProvider = new MongoDbDistinctValuesProvider(database, _environment);
            var randomValueProvider = new RandomValueProvider(_randomGenerator, columnPropertiesProvider, distinctValuesProvider);
            var queryBuilder = new MongoDbRawQueryBuilder(_query, randomValueProvider);

            return new MongoDbQueryExecutor(collection, queryBuilder, _environment);
        }
    }
}
