using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.Databases.Model;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using MySqlConnector;
using System.Diagnostics;

namespace DatabaseBenchmark.Databases.MySql
{
    public class MySqlDatabase: IDatabase
    {
        private const int DefaultImportBatchSize = 1000;

        private readonly string _connectionString;
        private readonly IExecutionEnvironment _environment;
        private readonly IOptionsProvider _optionsProvider;

        public MySqlDatabase(
            string connectionString,
            IExecutionEnvironment environment,
            IOptionsProvider optionsProvider)
        {
            _connectionString = connectionString;
            _environment = environment;
            _optionsProvider = optionsProvider;
        }

        public void CreateTable(Table table, bool dropExisting)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            if (dropExisting)
            {
                connection.DropTableIfExists(table.Name);
            }

            var tableBuilder = new MySqlTableBuilder(_optionsProvider);
            var commandText = tableBuilder.Build(table);
            var command = new MySqlCommand(commandText, connection);

            _environment.TraceCommand(command);

            command.ExecuteNonQuery();
        }

        public ImportResult ImportData(Table table, IDataSource source, int batchSize)
        {
            if (batchSize <= 0)
            {
                batchSize = DefaultImportBatchSize;
            }

            using var connection = new MySqlConnection(_connectionString);
            var parametersBuilder = new SqlNoParametersBuilder();
            var sourceReader = new DataSourceReader(source);
            var insertBuilder = new SqlInsertBuilder(table, sourceReader, parametersBuilder) { BatchSize = batchSize };
            var parameterAdapter = new MySqlParameterAdapter();
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
            var tableSize = GetTableSize(connection, table.Name);
            importResult.AddMetric(Metrics.TotalStorageBytes, tableSize);

            return importResult;
        }

        public IQueryExecutorFactory CreateQueryExecutorFactory(Table table, Query query) =>
            new SqlQueryExecutorFactory<MySqlConnection>(_connectionString, table, query, _environment)
                .Customize<ISqlQueryBuilder, MySqlQueryBuilder>()
                .Customize<ISqlParameterAdapter, MySqlParameterAdapter>();

        public IQueryExecutorFactory CreateRawQueryExecutorFactory(RawQuery query) =>
            new SqlRawQueryExecutorFactory<MySqlConnection>(_connectionString, query, _environment)
                .Customize<ISqlParameterAdapter, MySqlParameterAdapter>();

        public IQueryExecutorFactory CreateInsertExecutorFactory(Table table, IDataSource source) =>
            new SqlInsertExecutorFactory<MySqlConnection>(_connectionString, table, source, _environment)
                .Customize<ISqlParameterAdapter, MySqlParameterAdapter>();

        private static long GetTableSize(MySqlConnection connection, string tableName)
        {
            var command = new MySqlCommand($"SELECT data_length + index_length FROM information_schema.tables WHERE table_name = '{tableName}'", connection);
            return (long)(ulong)command.ExecuteScalar();
        }

        private static long GetRowCount(MySqlConnection connection, string tableName)
        {
            var command = new MySqlCommand($"SELECT COUNT(1) FROM {tableName}", connection);
            return (long)command.ExecuteScalar();
        }
    }
}
