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
            var parametersBuilder = new SqlParametersBuilder(':');
            var sourceReader = new DataSourceReader(source);
            var insertBuilder = new OracleInsertBuilder(table, sourceReader, parametersBuilder) { BatchSize = batchSize };
            var parameterAdapter = new OracleParameterAdapter();
            var insertExecutor = new SqlInsertExecutor(connection, insertBuilder, parametersBuilder, parameterAdapter, _environment);
            var transactionProvider = new SqlTransactionProvider(connection);
            var progressReporter = new ImportProgressReporter(_environment);
            var dataImporter = new DataImporter(
                insertExecutor,
                transactionProvider,
                progressReporter);

            var stopwatch = Stopwatch.StartNew();
            dataImporter.Import();
            stopwatch.Stop();

            var rowCount = GetRowCount(connection, table.Name);
            var importResult = new ImportResult(rowCount, stopwatch.ElapsedMilliseconds);

            return importResult;
        }

        public IQueryExecutorFactory CreateQueryExecutorFactory(Table table, Query query) =>
            new SqlQueryExecutorFactory<OracleConnection>(_connectionString, table, query, _environment)
                .Customize<ISqlParametersBuilder>(() => new SqlParametersBuilder(':'))
                .Customize<ISqlParameterAdapter, OracleParameterAdapter>();

        public IQueryExecutorFactory CreateRawQueryExecutorFactory(RawQuery query) =>
            new SqlRawQueryExecutorFactory<OracleConnection>(_connectionString, query, _environment)
                .Customize<ISqlParametersBuilder>(() => new SqlParametersBuilder(':'))
                .Customize<ISqlParameterAdapter, OracleParameterAdapter>();

        public IQueryExecutorFactory CreateInsertExecutorFactory(Table table, IDataSource source) =>
            new SqlInsertExecutorFactory<OracleConnection>(_connectionString, table, source, _environment)
                .Customize<ISqlParametersBuilder>(() => new SqlParametersBuilder(':'))
                .Customize<ISqlQueryBuilder, OracleInsertBuilder>()
                .Customize<ISqlParameterAdapter, OracleParameterAdapter>();

        private static long GetRowCount(OracleConnection connection, string tableName)
        {
            var command = new OracleCommand($"SELECT COUNT(1) FROM {tableName}", connection);
            return (long)(decimal)command.ExecuteScalar();
        }
    }
}
