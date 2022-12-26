using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using System.Data;

namespace DatabaseBenchmark.Databases.Sql
{
    public class SqlDataImporter
    {
        private readonly IDbConnection _connection;
        private readonly ISqlInsertBuilder _insertBuilder;
        private readonly ISqlParametersBuilder _parametersBuilder;
        private readonly ISqlParameterAdapter _parameterAdapter;
        private readonly IExecutionEnvironment _environment;

        public SqlDataImporter(
            IDbConnection connection,
            ISqlInsertBuilder insertBuilder,
            ISqlParametersBuilder parametersBuilder,
            ISqlParameterAdapter parameterAdapter,
            IExecutionEnvironment environment)
        {
            _connection = connection;
            _insertBuilder = insertBuilder;
            _parametersBuilder = parametersBuilder;
            _parameterAdapter = parameterAdapter;
            _environment = environment;
        }

        public void Import()
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            var progressReporter = new ImportProgressReporter(_environment);

            var transaction = _connection.BeginTransaction();
            try
            {
                int imported;
                do
                {
                    imported = ImportBatch(transaction);

                    progressReporter.Increment(imported);
                }
                while (imported == _insertBuilder.BatchSize);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private int ImportBatch(IDbTransaction transaction)
        {
            int rowCount = 0;

            if (_parametersBuilder != null)
            {
                _parametersBuilder.Reset();
            }

            var queryText = _insertBuilder.Build();

            if (queryText != null)
            {
                var command = _connection.CreateCommand();
                command.CommandText = queryText;

                if (transaction != null)
                {
                    command.Transaction = transaction;
                }

                foreach (var parameter in _parametersBuilder.Parameters)
                {
                    var dbParameter = command.CreateParameter();
                    _parameterAdapter.Populate(parameter, dbParameter);
                    command.Parameters.Add(dbParameter);
                }

                _environment.TraceCommand(command);

                rowCount = command.ExecuteNonQuery();
            }

            return rowCount;
        }
    }
}
