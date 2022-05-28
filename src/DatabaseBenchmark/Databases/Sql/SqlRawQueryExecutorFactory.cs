using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.Model;
using System.Data;

namespace DatabaseBenchmark.Databases.Sql
{
    public class SqlRawQueryExecutorFactory<TConnection> : IQueryExecutorFactory
        where TConnection: IDbConnection, new()
    {
        private readonly string _connectionString;
        private readonly RawQuery _query;
        private readonly IExecutionEnvironment _environment;
        private readonly RandomGenerator _randomGenerator;

        public SqlRawQueryExecutorFactory(
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
            var connection = new TConnection
            {
                ConnectionString = _connectionString
            };

            var parametersBuilder = new SqlParametersBuilder();
            var columnPropertiesProvider = new RawQueryColumnPropertiesProvider(_query);
            var distinctValuesProvider = new SqlDistinctValuesProvider(connection, _environment);
            var randomValueProvider = new RandomValueProvider(_randomGenerator, columnPropertiesProvider, distinctValuesProvider);
            var queryBuilder = new SqlRawQueryBuilder(_query, parametersBuilder, randomValueProvider);

            return new SqlQueryExecutor(connection, queryBuilder, parametersBuilder, _environment);
        }
    }
}
