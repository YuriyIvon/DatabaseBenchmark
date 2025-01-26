using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using Npgsql;

namespace DatabaseBenchmark.Databases.PostgreSql
{
    public class PostgreSqlDatabase : IDatabase
    {
        private readonly IExecutionEnvironment _environment;
        private readonly IOptionsProvider _optionsProvider;

        public string ConnectionString { get; }

        public PostgreSqlDatabase(
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

            var tableBuilder = new PostgreSqlTableBuilder();
            var commandText = tableBuilder.Build(table);
            var command = new NpgsqlCommand(commandText, connection);

            _environment.TraceCommand(command.CommandText);

            command.ExecuteNonQuery();
        }

        public IDataImporter CreateDataImporter(Table table, IDataSource source, int batchSize)
        {
            if (batchSize > 0)
            {
                _environment.WriteLine("Import batch size parameter is not used with PostgreSQL databases");
            }

            return new PostgreSqlDataImporter(ConnectionString, table, source, _environment);
        }

        public IQueryExecutorFactory CreateQueryExecutorFactory(Table table, Query query) =>
            new SqlQueryExecutorFactory<NpgsqlConnection>(this, table, query, _environment)
                .Customize<IOptionsProvider>(() => _optionsProvider)
                .Customize<IDistinctValuesProvider, PostgreSqlDistinctValuesProvider>()
                .Customize<ISqlQueryBuilder, PostgreSqlQueryBuilder>()
                .Customize<ISqlParameterAdapter, PostgreSqlParameterAdapter>();

        public IQueryExecutorFactory CreateRawQueryExecutorFactory(RawQuery query) =>
            new SqlRawQueryExecutorFactory<NpgsqlConnection>(this, query, _environment)
                .Customize<IDistinctValuesProvider, PostgreSqlDistinctValuesProvider>()
                .Customize<ISqlParameterAdapter, PostgreSqlParameterAdapter>();

        public IQueryExecutorFactory CreateInsertExecutorFactory(Table table, IDataSource source, int batchSize) =>
            new SqlInsertExecutorFactory<NpgsqlConnection>(this, table, source, batchSize, _environment)
                .Customize<ISqlParameterAdapter, PostgreSqlParameterAdapter>();

        public void ExecuteScript(string script)
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.ExecuteScript(script);
        }
    }
}
