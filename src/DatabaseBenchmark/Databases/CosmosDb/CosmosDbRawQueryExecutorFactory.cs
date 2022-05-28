using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Model;
using Microsoft.Azure.Cosmos;

namespace DatabaseBenchmark.Databases.CosmosDb
{
    public class CosmosDbRawQueryExecutorFactory : IQueryExecutorFactory
    {
        private readonly string _connectionString;
        private readonly string _databaseName;
        private readonly RawQuery _query;
        private readonly IExecutionEnvironment _environment;
        private readonly IOptionsProvider _optionsProvider;
        private readonly IRandomGenerator _randomGenerator;

        public CosmosDbRawQueryExecutorFactory( 
            string connectionString,
            string databaseName,
            RawQuery query,
            IExecutionEnvironment environment,
            IOptionsProvider optionsProvider)
        {
            _environment = environment;
            _connectionString = connectionString;
            _databaseName = databaseName;
            _query = query;
            _optionsProvider = optionsProvider;
            _randomGenerator = new RandomGenerator();
        }

        public IQueryExecutor Create()
        {
            var client = new CosmosClient(_connectionString);
            var database = client.GetDatabase(_databaseName);
            var container = database.GetContainer(_query.TableName);
            var columnPropertiesProvider = new RawQueryColumnPropertiesProvider(_query);
            var distinctValuesProvider = new CosmosDbDistinctValuesProvider(database, _environment);
            var randomValueProvider = new RandomValueProvider(_randomGenerator, columnPropertiesProvider, distinctValuesProvider);
            var parametersBuilder = new SqlParametersBuilder();
            var queryBuilder = new SqlRawQueryBuilder(_query, parametersBuilder, randomValueProvider);

            return new CosmosDbQueryExecutor(client, container, queryBuilder, parametersBuilder, _environment, _optionsProvider);
        }
    }
}
