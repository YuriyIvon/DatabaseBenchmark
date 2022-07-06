using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.Databases.Model;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using MonetDb.Mapi;
using System.Diagnostics;

namespace DatabaseBenchmark.Databases.MonetDb
{
    public class MonetDbDatabase : IDatabase
    {
        private const int DefaultImportBatchSize = 10;

        private readonly string _connectionString;
        private readonly IExecutionEnvironment _environment;

        public MonetDbDatabase(string connectionString, IExecutionEnvironment environment)
        {
            _connectionString = connectionString;
            _environment = environment;
        }

        public void CreateTable(Table table)
        {
            using var connection = new MonetDbConnection(_connectionString);
            connection.Open();

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
            connection.Open();

            var transaction = connection.BeginTransaction();

            try
            {
                var stopwatch = Stopwatch.StartNew();
                var progressReporter = new ImportProgressReporter(_environment);
                var dataImporter = new SqlDataImporter(_environment, progressReporter, true, batchSize);

                dataImporter.Import(source, table, connection, null);

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

        public IQueryExecutorFactory CreateQueryExecutorFactory(Table table, Query query)
        {
            return new SqlQueryExecutorFactory<MonetDbConnection>(
               _connectionString,
               table,
               _environment,
               (parametersBuilder, randomValueProvider) => new MonetDbQueryBuilder(table, query, parametersBuilder, randomValueProvider));
        }

        public IQueryExecutorFactory CreateRawQueryExecutorFactory(RawQuery query) =>
            new SqlRawQueryExecutorFactory<MonetDbConnection>(_connectionString, query, _environment);

        private static long GetTableSize(MonetDbConnection connection, string tableName)
        {
            var command = new MonetDbCommand($"SELECT SUM(columnsize) + SUM(heapsize) + SUM(hashes) + SUM(imprints) + SUM(orderidx) FROM storage() WHERE table = '{tableName}'", connection);
            return (long)command.ExecuteScalar();
        }

        private static long GetRowCount(MonetDbConnection connection, string tableName)
        {
            var command = new MonetDbCommand($"SELECT COUNT(1) FROM {tableName}", connection);
            return (long)command.ExecuteScalar();
        }
    }
}
