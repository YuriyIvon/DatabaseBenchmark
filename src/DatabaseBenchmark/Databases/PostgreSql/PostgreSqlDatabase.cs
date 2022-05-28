﻿using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.Databases.Model;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using Npgsql;
using System.Data;
using System.Diagnostics;

namespace DatabaseBenchmark.Databases.PostgreSql
{
    public class PostgreSqlDatabase : IDatabase
    {
        private readonly string _connectionString;
        private readonly IExecutionEnvironment _environment;

        public PostgreSqlDatabase(string connectionString, IExecutionEnvironment environment)
        {
            _connectionString = connectionString;
            _environment = environment;
        }

        public void CreateTable(Table table)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var tableBuilder = new PostgreSqlTableBuilder();
            var commandText = tableBuilder.Build(table);
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

            var columns = table.Columns.Where(c => !c.DatabaseGenerated).Select(x => x.Name).ToArray();

            var stopwatch = Stopwatch.StartNew();
            var progressReporter = new ImportProgressReporter(_environment);

            using (var writer = connection.BeginBinaryImport($"COPY {table.Name} ({string.Join(", ", columns)}) FROM STDIN (FORMAT BINARY)"))
            {
                while (source.Read())
                {
                    writer.StartRow();

                    foreach (var column in columns)
                    {
                        var value = source.GetValue(column);
                        writer.Write(value);
                    }

                    progressReporter.Increment(1);
                }

                writer.Complete();
            }

            stopwatch.Stop();

            return new ImportResult
            {
                Count = PostgreSqlDatabaseUtils.GetRowCount(connection, table.Name),
                Duration = stopwatch.ElapsedMilliseconds,
                TotalStorageBytes = PostgreSqlDatabaseUtils.GetTableSize(connection, table.Name)
            };
        }

        public IQueryExecutorFactory CreateQueryExecutorFactory(Table table, Query query)
        {
            return new SqlQueryExecutorFactory<NpgsqlConnection>(
               _connectionString,
               table,
               _environment,
               (parametersBuilder, randomValueProvider) => new SqlQueryBuilder(table, query, parametersBuilder, randomValueProvider));
        }

        public IQueryExecutorFactory CreateRawQueryExecutorFactory(RawQuery query) =>
            new SqlRawQueryExecutorFactory<NpgsqlConnection>(_connectionString, query, _environment);
    }
}
