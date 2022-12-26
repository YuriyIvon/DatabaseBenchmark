﻿using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.Databases.Model;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
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

        public void CreateTable(Table table, bool dropExisting)
        {
            using var connection = new ClickHouseConnection(_connectionString);
            connection.Open();

            if (dropExisting)
            {
                connection.DropTableIfExists(table.Name);
            }

            if (table.Columns.Any(c => c.DatabaseGenerated))
            {
                _environment.WriteLine("WARNING: ClickHouse doesn't support database-generated columns");
            }

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
            var parametersBuilder = new SqlNoParametersBuilder();
            var insertBuilder = new SqlInsertBuilder(table, source, parametersBuilder) { BatchSize = batchSize };
            var parameterAdapter = new ClickHouseParameterAdapter();
            var dataImporter = new SqlDataImporter(
                connection,
                insertBuilder,
                parametersBuilder,
                parameterAdapter,
                _environment);

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
            new SqlQueryExecutorFactory<ClickHouseConnection>(_connectionString, table, query, _environment)
                .Customize<ISqlQueryBuilder, ClickHouseQueryBuilder>()
                .Customize<ISqlParameterAdapter, ClickHouseParameterAdapter>();

        public IQueryExecutorFactory CreateRawQueryExecutorFactory(RawQuery query) =>
            new SqlRawQueryExecutorFactory<ClickHouseConnection>(_connectionString, query, _environment)
                .Customize<ISqlParameterAdapter, ClickHouseParameterAdapter>();

        public IQueryExecutorFactory CreateInsertExecutorFactory(Table table, IDataSource source) =>
            new SqlInsertExecutorFactory<ClickHouseConnection>(_connectionString, table, source, _environment)
                .Customize<ISqlParameterAdapter, ClickHouseParameterAdapter>();

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
