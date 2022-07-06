using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.Databases.Model;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using System.Data.SqlClient;
using System.Diagnostics;

namespace DatabaseBenchmark.Databases.SqlServer
{
    public class SqlServerDatabase : IDatabase
    {
        private const int DefaultImportBatchSize = 10;

        private readonly string _connectionString;
        private readonly IExecutionEnvironment _environment;

        public SqlServerDatabase(string connectionString, IExecutionEnvironment environment)
        {
            _connectionString = connectionString;
            _environment = environment;
        }

        public void CreateTable(Table table)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var tableBuilder = new SqlServerTableBuilder();
            var commandText = tableBuilder.Build(table);
            var command = new SqlCommand(commandText, connection);

            _environment.TraceCommand(command);

            command.ExecuteNonQuery();
        }

        public ImportResult ImportData(Table table, IDataSource source, int batchSize)
        {
            if (batchSize == 0)
            {
                batchSize = DefaultImportBatchSize;
            }

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var transaction = connection.BeginTransaction();
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var progressReporter = new ImportProgressReporter(_environment);
                var dataImporter = new SqlDataImporter(_environment, progressReporter, true, batchSize);

                dataImporter.Import(source, table, connection, transaction);

                transaction.Commit();
                stopwatch.Stop();

                var rowCount = GetRowCount(connection, table.Name);
                var importResult = new ImportResult(rowCount, stopwatch.ElapsedMilliseconds);
                var tableSize = GetTableSize(connection, table.Name);
                importResult.AddMetric(Metrics.TotalStorageBytes, tableSize);

                return importResult;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public IQueryExecutorFactory CreateQueryExecutorFactory(Table table, Query query) =>
            new SqlQueryExecutorFactory<SqlConnection>(
                _connectionString, 
                table, 
                _environment,
                (parametersBuilder, randomValueProvider) => new SqlQueryBuilder(table, query, parametersBuilder, randomValueProvider));

        public IQueryExecutorFactory CreateRawQueryExecutorFactory(RawQuery query) =>
            new SqlRawQueryExecutorFactory<SqlConnection>(_connectionString, query, _environment);

        private static long GetTableSize(SqlConnection connection, string tableName)
        {
            var command = new SqlCommand(@$"
SELECT SUM (s.used_page_count) * 8192 as used_size
FROM sys.dm_db_partition_stats AS s 
JOIN sys.tables AS t ON s.object_id = t.object_id
JOIN sys.indexes AS i ON i.object_id = t.object_id AND s.index_id = i.index_id
WHERE t.name = '{tableName}'",
                connection);
            return (long)command.ExecuteScalar();
        }

        public static long GetRowCount(SqlConnection connection, string tableName)
        {
            var command = new SqlCommand($"SELECT COUNT(1) FROM {tableName}", connection);
            return (int)command.ExecuteScalar();
        }
    }
}
