using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Model;
using System.Data;

namespace DatabaseBenchmark.Databases.Sql
{
    public class SqlQueryExecutorFactory<TConnection> : IQueryExecutorFactory
        where TConnection : IDbConnection, new()
    {
        private readonly string _connectionString;
        private readonly Table _table;
        private readonly IExecutionEnvironment _environment;
        private readonly Func<SqlParametersBuilder, IRandomValueProvider, ISqlQueryBuilder> _createQueryBuilder;
        private readonly IRandomGenerator _randomGenerator;

        public SqlQueryExecutorFactory(
            string connectionString,
            Table table,
            IExecutionEnvironment environment,
            Func<SqlParametersBuilder, IRandomValueProvider, ISqlQueryBuilder> createQueryBuilder)
        {
            _connectionString = connectionString;
            _table = table;
            _environment = environment;
            _createQueryBuilder = createQueryBuilder;
            _randomGenerator = new RandomGenerator();
        }

        public IQueryExecutor Create()
        {
            var connection = new TConnection
            {
                ConnectionString = _connectionString
            };

            var columnPropertiesProvider = new TableColumnPropertiesProvider(_table);
            var distinctValuesProvider = new SqlDistinctValuesProvider(connection, _environment);
            var randomValueProvider = new RandomValueProvider(_randomGenerator, columnPropertiesProvider, distinctValuesProvider);
            var parametersBuilder = new SqlParametersBuilder();
            var queryBuilder = _createQueryBuilder(parametersBuilder, randomValueProvider);

            return new SqlQueryExecutor(connection, queryBuilder, parametersBuilder, _environment);
        }
    }
}
