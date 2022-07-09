using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.Databases.Model;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using Npgsql;
using System.Data;
using System.Diagnostics;
using System.Text.Json;

namespace DatabaseBenchmark.Databases.PostgreSql
{
    public class PostgreSqlJsonbDatabase : IDatabase
    {
        private readonly string _connectionString;
        private readonly IExecutionEnvironment _environment;

        public PostgreSqlJsonbDatabase(string connectionString, IExecutionEnvironment environment)
        {
            _connectionString = connectionString;
            _environment = environment;
        }

        public void CreateTable(Table table, bool dropExisting)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            if (dropExisting)
            {
                connection.DropTableIfExists(table.Name);
            }

            var tableBuilder = new PostgreSqlJsonbTableBuilder();
            var commandText = tableBuilder.BuildCreateTableCommandText(table);
            var command = new NpgsqlCommand(commandText, connection);

            _environment.TraceCommand(command);

            command.ExecuteNonQuery();
        }

        public ImportResult ImportData(Table table, IDataSource source, int batchSize)
        {
            if (batchSize > 0)
            {
                _environment.WriteLine("Import batch size parameter is not used with PostgreSQL databases");
            }

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var columns = new List<string> { PostgreSqlJsonbConstants.JsonbColumnName };
            var nonQueryableColumns = table.Columns.Where(c => !c.Queryable && !c.DatabaseGenerated).Select(c => c.Name);
            columns.AddRange(nonQueryableColumns);

            var stopwatch = Stopwatch.StartNew();
            var progressReporter = new ImportProgressReporter(_environment);

            using (var writer = connection.BeginBinaryImport($"COPY {table.Name} ({string.Join(", ", columns)}) FROM STDIN (FORMAT BINARY)"))
            {
                while (source.Read())
                {
                    var jsonbValues = table.Columns
                        .Where(c => c.Queryable)
                        .ToDictionary(c => c.Name, c => source.GetValue(c.Name));

                    writer.StartRow();

                    writer.Write(JsonSerializer.Serialize(jsonbValues), NpgsqlTypes.NpgsqlDbType.Jsonb);

                    foreach (var column in nonQueryableColumns)
                    {
                        var value = source.GetValue(column);
                        writer.Write(value);
                    }

                    progressReporter.Increment(1);
                }

                writer.Complete();
            }

            stopwatch.Stop();

            var rowCount = PostgreSqlDatabaseUtils.GetRowCount(connection, table.Name);
            var importResult = new ImportResult(rowCount, stopwatch.ElapsedMilliseconds);
            var tableSize = PostgreSqlDatabaseUtils.GetTableSize(connection, table.Name);
            importResult.AddMetric(Metrics.TotalStorageBytes, tableSize);

            return importResult;
        }

        public IQueryExecutorFactory CreateQueryExecutorFactory(Table table, Query query) =>
             new SqlQueryExecutorFactory<NpgsqlConnection>(_connectionString, table, query, _environment)
                .Customize<ISqlQueryBuilder, PostgreSqlJsonbQueryBuilder>();

        public IQueryExecutorFactory CreateRawQueryExecutorFactory(RawQuery query) =>
            throw new NotImplementedException();
    }
}
