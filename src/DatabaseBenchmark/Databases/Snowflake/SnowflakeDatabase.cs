﻿using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Interfaces;
using DatabaseBenchmark.Databases.Model;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Databases.SqlServer;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using Snowflake.Data.Client;
using System.Data.SqlClient;
using System.Diagnostics;

namespace DatabaseBenchmark.Databases.Snowflake
{
    public class SnowflakeDatabase : IDatabase
    {
        private const int DefaultImportBatchSize = 10000;

        private readonly string _connectionString;
        private readonly IExecutionEnvironment _environment;

        public SnowflakeDatabase(string connectionString, IExecutionEnvironment environment)
        {
            _connectionString = connectionString;
            _environment = environment;

            SnowflakeDbConnectionPool.SetPooling(false);
        }

        public void CreateTable(Table table, bool dropExisting)
        {
            using var connection = new SnowflakeDbConnection
            {
                ConnectionString = _connectionString
            };

            connection.Open();

            if (dropExisting)
            {
                connection.DropTableIfExists(table.Name);
            }

            var tableBuilder = new SnowflakeTableBuilder();
            var commandText = tableBuilder.Build(table);
            var command = new SnowflakeDbCommand(connection)
            {
                CommandText = commandText
            };

            _environment.TraceCommand(command);

            command.ExecuteNonQuery();
        }

        public ImportResult ImportData(Table table, IDataSource source, int batchSize)
        {
            if (batchSize <= 0)
            {
                batchSize = DefaultImportBatchSize;
            }

            using var connection = new SnowflakeDbConnection();
            connection.ConnectionString = _connectionString;
            connection.Open();

            var stopwatch = Stopwatch.StartNew();
            var progressReporter = new ImportProgressReporter(_environment);
            var dataImporter = new SqlDataImporter(_environment, progressReporter, batchSize);

            var transaction = connection.BeginTransaction();
            try
            {
                dataImporter.Import(source, table, connection, transaction);

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

            return importResult;
        }

        public IQueryExecutorFactory CreateQueryExecutorFactory(Table table, Query query) =>
             new SqlQueryExecutorFactory<SnowflakeDbConnection>(_connectionString, table, query, _environment)
                .Customize<ISqlQueryBuilder, SnowflakeQueryBuilder>()
                .Customize<SqlQueryParametersBuilder>(() => new SqlQueryParametersBuilder(':'))
                .Customize<ISqlParameterAdapter, SnowflakeParameterAdapter>();

        public IQueryExecutorFactory CreateRawQueryExecutorFactory(RawQuery query) =>
            new SqlRawQueryExecutorFactory<SnowflakeDbConnection>(_connectionString, query, _environment)
                .Customize<SqlQueryParametersBuilder>(() => new SqlQueryParametersBuilder(':'))
                .Customize<ISqlParameterAdapter, SnowflakeParameterAdapter>();

        private static long GetRowCount(SnowflakeDbConnection connection, string tableName)
        {
            var command = new SnowflakeDbCommand(connection)
            {
                CommandText = $"SELECT COUNT(1) FROM {tableName}"
            };

            return (long)command.ExecuteScalar();
        }
    }
}