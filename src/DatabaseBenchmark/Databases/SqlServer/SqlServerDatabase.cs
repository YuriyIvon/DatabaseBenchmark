using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.Databases.Model;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using System.Data.SqlClient;
using System.Diagnostics;

namespace DatabaseBenchmark.Databases.SqlServer
{
    public class SqlServerDatabase : IDatabase
    {
        private const int DefaultImportBatchSize = 100000;

        private readonly string _connectionString;
        private readonly IExecutionEnvironment _environment;

        public SqlServerDatabase(string connectionString, IExecutionEnvironment environment)
        {
            _connectionString = connectionString;
            _environment = environment;
        }

        public void CreateTable(Table table, bool dropExisting)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            if (dropExisting)
            {
                connection.DropTableIfExists(table.Name);
            }

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

            var stopwatch = Stopwatch.StartNew();
            var progressReporter = new ImportProgressReporter(_environment);

            var transaction = connection.BeginTransaction();
            try
            {
                using var dataReaderAdapter = new DataReaderAdapter(source, table);
                using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction);
                bulkCopy.DestinationTableName = table.Name;
                bulkCopy.BatchSize = batchSize;
                bulkCopy.NotifyAfter = batchSize;
                bulkCopy.SqlRowsCopied += (s, e) => progressReporter.Increment(batchSize);

                var sourceColumns = table.Columns.Where(c => !c.DatabaseGenerated).ToList();
                sourceColumns.ForEach(c => bulkCopy.ColumnMappings.Add(c.Name, c.Name));

                bulkCopy.WriteToServer(dataReaderAdapter);

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
            var tableSize = GetTableSize(connection, table.Name);
            importResult.AddMetric(Metrics.TotalStorageBytes, tableSize);

            return importResult;
        }

        public IQueryExecutorFactory CreateQueryExecutorFactory(Table table, Query query) =>
            new SqlQueryExecutorFactory<SqlConnection>(_connectionString, table, query, _environment);

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

        private static long GetRowCount(SqlConnection connection, string tableName)
        {
            var command = new SqlCommand($"SELECT COUNT(1) FROM {tableName}", connection);
            return (int)command.ExecuteScalar();
        }
    }
}
