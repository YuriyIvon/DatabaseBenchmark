using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using MySqlConnector;

namespace DatabaseBenchmark.Databases.MySql
{
    public class MySqlDatabase: IDatabase
    {
        private const int DefaultImportBatchSize = 1000;

        private readonly IExecutionEnvironment _environment;
        private readonly IOptionsProvider _optionsProvider;

        public string ConnectionString { get; }

        public MySqlDatabase(
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
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();

            if (dropExisting)
            {
                connection.DropTableIfExists(table.Name);
            }

            var tableBuilder = new MySqlTableBuilder(_optionsProvider);
            var commandText = tableBuilder.Build(table);
            var command = new MySqlCommand(commandText, connection);

            _environment.TraceCommand(command);

            command.ExecuteNonQuery();
        }

        public IDataImporter CreateDataImporter(Table table, IDataSource source, int batchSize) =>
            new SqlDataImporterBuilder(table, source, batchSize, DefaultImportBatchSize)
                .Connection<MySqlConnection>(ConnectionString)
                .ParametersBuilder<SqlNoParametersBuilder>()
                .ParameterAdapter<MySqlParameterAdapter>()
                .TransactionProvider<SqlTransactionProvider>()
                .DataMetricsProvider<SqlDataMetricsProvider>(dmp =>
                    dmp.AddMetric(Metrics.TotalStorageBytes, $"SELECT data_length + index_length FROM information_schema.tables WHERE table_name = '{table.Name}'"))
                .ProgressReporter<ImportProgressReporter>()
                .OptionsProvider(_optionsProvider)
                .Environment(_environment)
                .Build();

        public IQueryExecutorFactory CreateQueryExecutorFactory(Table table, Query query) =>
            new SqlQueryExecutorFactory<MySqlConnection>(this, table, query, _environment)
                .Customize<ISqlQueryBuilder, MySqlQueryBuilder>()
                .Customize<ISqlParameterAdapter, MySqlParameterAdapter>();

        public IQueryExecutorFactory CreateRawQueryExecutorFactory(RawQuery query) =>
            new SqlRawQueryExecutorFactory<MySqlConnection>(this, query, _environment)
                .Customize<ISqlParameterAdapter, MySqlParameterAdapter>();

        public IQueryExecutorFactory CreateInsertExecutorFactory(Table table, IDataSource source, int batchSize) =>
            new SqlInsertExecutorFactory<MySqlConnection>(this, table, source, batchSize, _environment)
                .Customize<ISqlParameterAdapter, MySqlParameterAdapter>();

        public void ExecuteScript(string script)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.ExecuteScript(script);
        }
    }
}
