using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.Databases.Model;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using MonetDb.Mapi;
using System.Diagnostics;

namespace DatabaseBenchmark.Databases.MonetDb
{
    public class MonetDbDatabase : IDatabase
    {
        private const int DefaultImportBatchSize = 100;

        private readonly string _connectionString;
        private readonly IExecutionEnvironment _environment;

        public MonetDbDatabase(string connectionString, IExecutionEnvironment environment)
        {
            _connectionString = connectionString;
            _environment = environment;
        }

        public void CreateTable(Table table, bool dropExisting)
        {
            using var connection = new MonetDbConnection(_connectionString);
            connection.Open();

            if (dropExisting)
            {
                connection.DropTableIfExists(table.Name);
            }

            var tableBuilder = new MonetDbTableBuilder();
            var commandText = tableBuilder.Build(table);
            var command = new MonetDbCommand(commandText, connection);

            _environment.TraceCommand(command);

            command.ExecuteNonQuery();
        }

        public ImportResult ImportData(Table table, IDataSource source, int batchSize)
        {
            if (batchSize <= 0)
            {
                batchSize = DefaultImportBatchSize;
            }

            using var connection = new MonetDbConnection(_connectionString);
            var parametersBuilder = new SqlNoParametersBuilder();
            var sourceReader = new DataSourceReader(source);
            var insertBuilder = new SqlInsertBuilder(table, sourceReader, parametersBuilder) { BatchSize = batchSize };
            var parameterAdapter = new MonetDbParameterAdapter();
            var insertExecutor = new SqlInsertExecutor(connection, insertBuilder, parametersBuilder, parameterAdapter, _environment);
            var transactionProvider = new MonetDbTransactionProvider(connection);
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
            new SqlQueryExecutorFactory<MonetDbConnection>(_connectionString, table, query, _environment)
                .Customize<ISqlQueryBuilder, MonetDbQueryBuilder>()
                .Customize<ISqlParameterAdapter, MonetDbParameterAdapter>();

        public IQueryExecutorFactory CreateRawQueryExecutorFactory(RawQuery query) =>
            new SqlRawQueryExecutorFactory<MonetDbConnection>(_connectionString, query, _environment)
                .Customize<ISqlParameterAdapter, MonetDbParameterAdapter>();

        public IQueryExecutorFactory CreateInsertExecutorFactory(Table table, IDataSource source) =>
            new SqlInsertExecutorFactory<MonetDbConnection>(_connectionString, table, source, _environment)
                .Customize<ISqlParameterAdapter, MonetDbParameterAdapter>();

        private static long GetTableSize(MonetDbConnection connection, string tableName)
        {
            var command = new MonetDbCommand($"SELECT SUM(columnsize) + SUM(heapsize) + SUM(hashes) + SUM(imprints) + SUM(orderidx) FROM storage() WHERE table = '{tableName.ToLower()}'", connection);
            return (long)command.ExecuteScalar();
        }

        private static long GetRowCount(MonetDbConnection connection, string tableName)
        {
            var command = new MonetDbCommand($"SELECT COUNT(1) FROM {tableName}", connection);
            return (long)command.ExecuteScalar();
        }
    }
}
