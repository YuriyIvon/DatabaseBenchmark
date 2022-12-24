﻿using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using System.Data;

namespace DatabaseBenchmark.Databases.Sql
{
    public sealed class SqlQueryExecutor : IQueryExecutor
    {
        private readonly IDbConnection _connection;
        private readonly ISqlQueryBuilder _queryBuilder;
        private readonly SqlQueryParametersBuilder _parametersBuilder;
        private readonly ISqlParameterAdapter _parameterAdapter;
        private readonly IExecutionEnvironment _environment;

        public SqlQueryExecutor(
            IDbConnection connection,
            ISqlQueryBuilder queryBuilder,
            SqlQueryParametersBuilder parametersBuilder,
            ISqlParameterAdapter parameterAdapter,
            IExecutionEnvironment environment)
        {
            _connection = connection;
            _queryBuilder = queryBuilder;
            _parametersBuilder = parametersBuilder;
            _parameterAdapter = parameterAdapter;
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

            foreach (var parameter in _parametersBuilder.Parameters)
            {
                var dbParameter = command.CreateParameter();
                _parameterAdapter.Populate(parameter, dbParameter);
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
