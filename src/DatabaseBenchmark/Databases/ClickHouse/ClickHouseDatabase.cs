using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.Databases.Model;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using Octonica.ClickHouseClient;
using System.Diagnostics;

namespace DatabaseBenchmark.Databases.ClickHouse
{
    public class ClickHouseDatabase : IDatabase
    {
        private const int DefaultImportBatchSize = 1000;

        private readonly string _connectionString;
        private readonly IExecutionEnvironment _environment;
        private readonly IOptionsProvider _optionsProvider;

        public ClickHouseDatabase(
            string connectionString,
            IExecutionEnvironment environment,
            IOptionsProvider optionsProvider)
        {
            _connectionString = connectionString;
            _environment = environment;
            _optionsProvider = optionsProvider;
        }

        public void CreateTable(Table table)
        {
            using var connection = new ClickHouseConnection(_connectionString);
            connection.Open();

            var tableBuilder = new ClickHouseTableBuilder(_optionsProvider);
            var commandText = tableBuilder.Build(table);
            var command = connection.CreateCommand(commandText);

            _environment.TraceCommand(command);

            command.ExecuteNonQuery();
        }

        public ImportResult ImportData(Table table, IDataSource source, int batchSize)
        {
            if (batchSize <= 0)
            {
                batchSize = DefaultImportBatchSize;
            }

            using var connection = new ClickHouseConnection(_connectionString);
            connection.Open();

            var stopwatch = Stopwatch.StartNew();
            var progressReporter = new ImportProgressReporter(_environment);
            var dataImporter = new SqlDataImporter(_environment, progressReporter, false, batchSize);

            dataImporter.Import(source, table, connection, null);

            stopwatch.Stop();

            var rowCount = GetRowCount(connection, table.Name);
            var importResult = new ImportResult(rowCount, stopwatch.ElapsedMilliseconds);
            var tableSize = GetTableSize(connection, table.Name);
            importResult.AddMetric(Metrics.TotalStorageBytes, tableSize);

            return importResult;
        }

        public IQueryExecutorFactory CreateQueryExecutorFactory(Table table, Query query) =>
            new SqlQueryExecutorFactory<ClickHouseConnection>(_connectionString, table, _environment,
                (parametersBuilder, randomValueProvider) => new ClickHouseQueryBuilder(table, query, parametersBuilder, randomValueProvider));

        public IQueryExecutorFactory CreateRawQueryExecutorFactory(RawQuery query) =>
            new SqlRawQueryExecutorFactory<ClickHouseConnection>(_connectionString, query, _environment);

        private static long GetRowCount(ClickHouseConnection connection, string tableName)
        {
            var command = connection.CreateCommand($"SELECT COUNT(1) FROM {tableName}");
            return (long)(ulong)command.ExecuteScalar();
        }

        private static long GetTableSize(ClickHouseConnection connection, string tableName)
        {
            var command = connection.CreateCommand($"SELECT SUM(bytes) FROM system.parts WHERE active AND table = '{tableName}'");
            return (long)(ulong)command.ExecuteScalar();
        }
    }
}
