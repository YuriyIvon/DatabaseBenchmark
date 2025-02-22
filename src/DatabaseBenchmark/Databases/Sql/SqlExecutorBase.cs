﻿using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using System.Data;

namespace DatabaseBenchmark.Databases.Sql
{
    public abstract class SqlExecutorBase : IQueryExecutor
    {
        private readonly IDbConnection _connection;
        private readonly ISqlQueryBuilder _queryBuilder;
        private readonly ISqlParametersBuilder _parametersBuilder;
        private readonly ISqlParameterAdapter _parameterAdapter;
        private readonly IExecutionEnvironment _environment;

        protected SqlExecutorBase(
            IDbConnection connection,
            ISqlQueryBuilder queryBuilder,
            ISqlParametersBuilder parametersBuilder,
            ISqlParameterAdapter parameterAdapter,
            IExecutionEnvironment environment)
        {
            _connection = connection;
            _queryBuilder = queryBuilder;
            _parametersBuilder = parametersBuilder;
            _parameterAdapter = parameterAdapter;
            _environment = environment;
        }

        public IPreparedQuery Prepare() => Prepare(null);

        public IPreparedQuery Prepare(ITransaction transaction)
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

            ApplyTransaction(transaction, command);

            _environment.TraceCommand(command.CommandText, _parametersBuilder.Parameters);

            return CreatePreparedStatement(command);
        }

        public void Dispose() => _connection?.Dispose();

        protected abstract IPreparedQuery CreatePreparedStatement(IDbCommand command);

        private void ApplyTransaction(ITransaction transaction, IDbCommand command)
        {
            if (transaction != null)
            {
                if (transaction is ISqlTransaction sqlTransaction)
                {
                    if (sqlTransaction.Transaction != null)
                    {
                        command.Transaction = sqlTransaction.Transaction;
                    }
                }
                else
                {
                    _environment.WriteLine("WARNING: ignoring an incompatible transaction object");
                }
            }
        }
    }
}
