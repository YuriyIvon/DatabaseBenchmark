using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.Databases.Model;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using Oracle.ManagedDataAccess.Client;
using System.Diagnostics;

namespace DatabaseBenchmark.Databases.Oracle
{
    public class OracleDatabase : IDatabase
    {
        private const int DefaultImportBatchSize = 10;

        private readonly string _connectionString;
        private readonly IExecutionEnvironment _environment;

        public OracleDatabase(string connectionString, IExecutionEnvironment environment)
        {
            _connectionString = connectionString;
            _environment = environment;
        }

        public void CreateTable(Table table, bool dropExisting)
        {
            using var connection = new OracleConnection(_connectionString);
            connection.Open();

            if (dropExisting)
            {
                connection.DropTableIfExists(table.Name);
            }

            var tableBuilder = new OracleTableBuilder();
            var commandText = tableBuilder.Build(table);
            var command = new OracleCommand(commandText, connection);

            _environment.TraceCommand(command);

            command.ExecuteNonQuery();
        }

        public ImportResult ImportData(Table table, IDataSource source, int batchSize)
        {
            if (batchSize == 0)
            {
                batchSize = DefaultImportBatchSize;
            }

            using var connection = new OracleConnection(_connectionString);
            connection.Open();

            var stopwatch = Stopwatch.StartNew();
            var progressReporter = new ImportProgressReporter(_environment);
            var parametersBuilder = new SqlParametersBuilder(":");
            var dataImporter = new OracleDataImporter(_environment, progressReporter, parametersBuilder, batchSize);

            var transaction = connection.BeginTransaction();
            try
            {
                dataImporter.Import(source, table, connection, transaction);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            stopwatch.Stop();

            var rowCount = GetRowCount(connection, table.Name);
            var importResult = new ImportResult(rowCount, stopwatch.ElapsedMilliseconds);

            return importResult;
        }

        public IQueryExecutorFactory CreateQueryExecutorFactory(Table table, Query query) =>
            new SqlQueryExecutorFactory<OracleConnection>(_connectionString, table, query, _environment)
                .Customize<SqlParametersBuilder>(() => new SqlParametersBuilder(":"));

        public IQueryExecutorFactory CreateRawQueryExecutorFactory(RawQuery query) =>
            new SqlRawQueryExecutorFactory<OracleConnection>(_connectionString, query, _environment)
                .Customize<SqlParametersBuilder>(() => new SqlParametersBuilder(":"));

        private static long GetRowCount(OracleConnection connection, string tableName)
        {
            var command = new OracleCommand($"SELECT COUNT(1) FROM {tableName}", connection);
            return (long)(decimal)command.ExecuteScalar();
        }
    }
}
