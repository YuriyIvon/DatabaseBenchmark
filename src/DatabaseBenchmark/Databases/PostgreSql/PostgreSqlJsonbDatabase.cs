using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
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
        private readonly IExecutionEnvironment _environment;
        private readonly IOptionsProvider _optionsProvider;

        public string ConnectionString { get; }

        public PostgreSqlJsonbDatabase(
            string connectionString,
            IExecutionEnvironment environment,
            IOptionsProvider optionsProvider)
        {
            ConnectionString = connectionString;
            _environment = environment;
            _optionsProvider = optionsProvider;
        }

        public void CreateTable(Table table, bool dropExisting)
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            if (dropExisting)
            {
                connection.DropTableIfExists(table.Name);
            }

            var tableBuilder = new PostgreSqlJsonbTableBuilder(_optionsProvider);
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

            return new PostgreSqlJsonbDataImporter(ConnectionString, table, source, _environment);
        }

        public IQueryExecutorFactory CreateQueryExecutorFactory(Table table, Query query) =>
             new SqlQueryExecutorFactory<NpgsqlConnection>(this, table, query, _environment)
                .Customize<IOptionsProvider>(() => _optionsProvider)
                .Customize<IDistinctValuesProvider, PostgreSqlJsonbDistinctValuesProvider>()
                .Customize<ISqlQueryBuilder, PostgreSqlJsonbQueryBuilder>()
                .Customize<ISqlParameterAdapter, PostgreSqlJsonbParameterAdapter>();

        public IQueryExecutorFactory CreateRawQueryExecutorFactory(RawQuery query) =>
            throw new NotSupportedException();

        public IQueryExecutorFactory CreateInsertExecutorFactory(Table table, IDataSource source, int batchSize) =>
             new SqlInsertExecutorFactory<NpgsqlConnection>(this, table, source, batchSize, _environment)
                .Customize<ISqlQueryBuilder, PostgreSqlJsonbInsertBuilder>()
                .Customize<ISqlParameterAdapter, PostgreSqlJsonbParameterAdapter>();

        public void ExecuteScript(string script)
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.ExecuteScript(script);
        }
    }
}
