using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Model;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using Npgsql;
using Pgvector;
using System.Diagnostics;

namespace DatabaseBenchmark.Databases.PostgreSql
{
    public sealed class PostgreSqlDataImporter : IDataImporter
    {
        private readonly string _connectionString;
        private readonly Table _table;
        private readonly IDataSource _source;
        private readonly IExecutionEnvironment _environment;

        public PostgreSqlDataImporter(
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
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(_connectionString);
            dataSourceBuilder.UseVector();

            using var dataSource = dataSourceBuilder.Build();
            using var connection = dataSource.OpenConnection();

            var columns = _table.Columns.Where(c => !c.DatabaseGenerated).ToArray();
            var columnNames = columns.Select(c => c.Name).ToArray();

            var stopwatch = Stopwatch.StartNew();
            var progressReporter = new ImportProgressReporter(_environment);

            using (var writer = connection.BeginBinaryImport($"COPY {_table.Name} ({string.Join(", ", columnNames)}) FROM STDIN (FORMAT BINARY)"))
            {
                while (_source.Read())
                {
                    writer.StartRow();

                    foreach (var column in columns)
                    {
                        var value = _source.GetValue(column.Name);

                        if (column.Type == ColumnType.Vector)
                        {
                            var vector = new Vector((float[])TypeConverter.ChangeType(value, typeof(float[])));
                            writer.Write(vector);
                        }
                        else
                        {
                            var nativeColumnType = PostgreSqlDatabaseUtils.GetNativeColumnType(column.Type, column.Array);
                            writer.Write(value, nativeColumnType);
                        }
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
