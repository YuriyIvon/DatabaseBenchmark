using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Model;
using Microsoft.Azure.Cosmos;

namespace DatabaseBenchmark.Databases.CosmosDb
{
    public class CosmosDbQueryExecutorFactory : IQueryExecutorFactory
    {
        private readonly string _connectionString;
        private readonly string _databaseName;
        private readonly Table _table;
        private readonly Query _query;
        private readonly IExecutionEnvironment _environment;
        private readonly IOptionsProvider _optionsProvider;
        private readonly IRandomGenerator _randomGenerator;

        public CosmosDbQueryExecutorFactory(
            string connectionString,
            string databaseName,
            Table table,
            Query query,
            IExecutionEnvironment environment,
            IOptionsProvider optionsProvider)
        {
            _connectionString = connectionString;
            _databaseName = databaseName;
            _table = table;
            _query = query;
            _environment = environment;
            _optionsProvider = optionsProvider;
            _randomGenerator = new RandomGenerator();
        }

        public IQueryExecutor Create()
        {
            var client = new CosmosClient(_connectionString);
            var database = client.GetDatabase(_databaseName);
            var container = database.GetContainer(_table.Name);
            var columnPropertiesProvider = new TableColumnPropertiesProvider(_table);
            var distinctValuesProvider = new CosmosDbDistinctValuesProvider(database, _environment);
            var randomValueProvider = new RandomValueProvider(_randomGenerator, columnPropertiesProvider, distinctValuesProvider);
            var parametersBuilder = new SqlParametersBuilder();
            var queryBuilder = new CosmosDbQueryBuilder(_table, _query, parametersBuilder, randomValueProvider);

            return new CosmosDbQueryExecutor(client, container, queryBuilder, parametersBuilder, _environment, _optionsProvider);
        }
    }
}
