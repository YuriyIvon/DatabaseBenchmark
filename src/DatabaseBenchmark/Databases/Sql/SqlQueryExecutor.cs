using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using System.Data;

namespace DatabaseBenchmark.Databases.Sql
{
    public class SqlQueryExecutor : IQueryExecutor
    {
        private readonly IDbConnection _connection;
        private readonly ISqlQueryBuilder _queryBuilder;
        private readonly SqlParametersBuilder _parametersBuilder;
        private readonly IExecutionEnvironment _environment;

        public SqlQueryExecutor(
            IDbConnection connection,
            ISqlQueryBuilder queryBuilder,
            SqlParametersBuilder parametersBuilder,
            IExecutionEnvironment environment)
        {
            _connection = connection;
            _queryBuilder = queryBuilder;
            _parametersBuilder = parametersBuilder;
            _environment = environment;
        }

        public IPreparedQuery Prepare()
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            var queryText = _queryBuilder.Build();

            var command = _connection.CreateCommand();
            command.CommandText = queryText;

            foreach (var parameter in _parametersBuilder.Values)
            {
                var dbParameter = command.CreateParameter();
                dbParameter.ParameterName = parameter.Key;
                dbParameter.Value = parameter.Value;
                command.Parameters.Add(dbParameter);
            }

            _environment.TraceCommand(command);

            return new SqlPreparedQuery(command);
        }

        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Dispose();
            }
        }
    }
}
