using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Model;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using System.Data.SqlClient;
using System.Diagnostics;

namespace DatabaseBenchmark.Databases.SqlServer
{
    public sealed class SqlServerDataImporter : IDataImporter
    {
        private const int DefaultImportBatchSize = 100000;

        private readonly IExecutionEnvironment _environment;
        private readonly string _connectionString;
        private readonly Table _table;
        private readonly IDataSource _source;
        private readonly int _batchSize;

        public SqlServerDataImporter(
            string connectionString,
            Table table,
            IDataSource source,
            int batchSize,
            IExecutionEnvironment environment)
        {
            _connectionString = connectionString;
            _table = table;
            _source = source;
            _batchSize = batchSize == 0 ? DefaultImportBatchSize : batchSize;
            _environment = environment;
        }

        public ImportResult Import()
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            var stopwatch = Stopwatch.StartNew();
            var progressReporter = new ImportProgressReporter(_environment);

            var transaction = connection.BeginTransaction();
            try
            {
                using var dataReaderAdapter = new DataReaderAdapter(_table, _source);
                using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction);
                bulkCopy.DestinationTableName = _table.Name;
                bulkCopy.BatchSize = _batchSize;
                bulkCopy.NotifyAfter = _batchSize;
                bulkCopy.SqlRowsCopied += (s, e) => progressReporter.Increment(_batchSize);

                var sourceColumns = _table.Columns.Where(c => !c.DatabaseGenerated).ToList();
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

            var dataMetricsProvider = new SqlDataMetricsProvider(connection, _table);
            var rowCount = dataMetricsProvider.GetRowCount();
            var importResult = new ImportResult(rowCount, stopwatch.ElapsedMilliseconds);
            var tableSize = GetTableSize(connection, _table.Name);
            importResult.AddMetric(Metrics.TotalStorageBytes, tableSize);

            return importResult;
        }

        public void Dispose()
        {
        }

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
    }
}
