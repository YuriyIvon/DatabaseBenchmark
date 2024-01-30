using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Model;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using Npgsql;
using System.Diagnostics;
using System.Text.Json;

namespace DatabaseBenchmark.Databases.PostgreSql
{
    public sealed class PostgreSqlJsonbDataImporter : IDataImporter
    {
        private readonly string _connectionString;
        private readonly Table _table;
        private readonly IDataSource _source;
        private readonly IExecutionEnvironment _environment;

        public PostgreSqlJsonbDataImporter(
            string connectionString,
            Table table,
            IDataSource source,
            IExecutionEnvironment environment)
        {
            _connectionString = connectionString;
            _table = table;
            _source = source;
            _environment = environment;
        }

        public ImportResult Import()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var columnNames = new List<string> { PostgreSqlJsonbConstants.JsonbColumnName };
            var nonQueryableColumns = _table.Columns.Where(c => !c.Queryable && !c.DatabaseGenerated).ToArray();
            columnNames.AddRange(nonQueryableColumns.Select(c => c.Name));

            var stopwatch = Stopwatch.StartNew();
            var progressReporter = new ImportProgressReporter(_environment);

            using (var writer = connection.BeginBinaryImport($"COPY {_table.Name} ({string.Join(", ", columnNames)}) FROM STDIN (FORMAT BINARY)"))
            {
                while (_source.Read())
                {
                    var jsonbValues = _table.Columns
                        .Where(c => c.Queryable)
                        .ToDictionary(
                            c => c.Name,
                            c => _source.GetValue(c.Name));
                    writer.StartRow();

                    writer.Write(JsonSerializer.Serialize(jsonbValues), NpgsqlTypes.NpgsqlDbType.Jsonb);

                    foreach (var column in nonQueryableColumns)
                    {
                        var value = _source.GetValue(column.Name);
                        writer.Write(value);
                    }

                    progressReporter.Increment(1);
                }

                writer.Complete();
            }

            stopwatch.Stop();

            var dataMetricsProvider = new SqlDataMetricsProvider(connection, _table);
            var rowCount = dataMetricsProvider.GetRowCount();
            var importResult = new ImportResult(rowCount, stopwatch.ElapsedMilliseconds);
            var tableSize = PostgreSqlDatabaseUtils.GetTableSize(connection, _table.Name);
            importResult.AddMetric(Metrics.TotalStorageBytes, tableSize);

            return importResult;
        }

        public void Dispose()
        {
        }
    }
}
