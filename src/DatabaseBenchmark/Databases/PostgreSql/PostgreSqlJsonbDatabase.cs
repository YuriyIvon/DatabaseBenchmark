﻿using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using Npgsql;

namespace DatabaseBenchmark.Databases.PostgreSql
{
    public class PostgreSqlJsonbDatabase : IDatabase
    {
        private readonly string _connectionString;
        private readonly IExecutionEnvironment _environment;
        private readonly IOptionsProvider _optionsProvider;

        public PostgreSqlJsonbDatabase(
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

        public IDataImporter CreateDataImporter(Table table, IDataSource source, int batchSize)
        {
            if (batchSize > 0)
            {
                _environment.WriteLine("Import batch size parameter is not used with PostgreSQL databases");
            }

            return new PostgreSqlJsonbDataImporter(_connectionString, table, source, _environment);
        }

        public IQueryExecutorFactory CreateQueryExecutorFactory(Table table, Query query) =>
             new SqlQueryExecutorFactory<NpgsqlConnection>(_connectionString, table, query, _environment)
                .Customize<IOptionsProvider>(() => _optionsProvider)
                .Customize<IDistinctValuesProvider, PostgreSqlJsonbDistinctValuesProvider>()
                .Customize<ISqlQueryBuilder, PostgreSqlJsonbQueryBuilder>()
                .Customize<ISqlParameterAdapter, PostgreSqlJsonbParameterAdapter>();

        public IQueryExecutorFactory CreateRawQueryExecutorFactory(RawQuery query) =>
            throw new NotImplementedException();

        public IQueryExecutorFactory CreateInsertExecutorFactory(Table table, IDataSource source, int batchSize) =>
             new SqlInsertExecutorFactory<NpgsqlConnection>(_connectionString, table, source, batchSize, _environment)
                .Customize<ISqlQueryBuilder, PostgreSqlJsonbInsertBuilder>()
                .Customize<ISqlParameterAdapter, PostgreSqlJsonbParameterAdapter>();
    }
}
