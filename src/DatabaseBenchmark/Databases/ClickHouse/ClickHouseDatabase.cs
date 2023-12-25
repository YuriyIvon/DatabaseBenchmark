using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using Octonica.ClickHouseClient;

namespace DatabaseBenchmark.Databases.ClickHouse
{
    public class ClickHouseDatabase : IDatabase
    {
        private const int DefaultImportBatchSize = 1000;

        private readonly IExecutionEnvironment _environment;
        private readonly IOptionsProvider _optionsProvider;

        public string ConnectionString { get; }

        public ClickHouseDatabase(
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
            using var connection = new ClickHouseConnection(ConnectionString);
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

        public IDataImporter CreateDataImporter(Table table, IDataSource source, int batchSize) =>
            new SqlDataImporterBuilder(table, source, batchSize, DefaultImportBatchSize)
                .Connection<ClickHouseConnection>(ConnectionString)
                .ParametersBuilder<SqlNoParametersBuilder>()
                .ParameterAdapter<ClickHouseParameterAdapter>()
                .OptionsProvider(_optionsProvider)
                .Environment(_environment)
                .TransactionProvider<NoTransactionProvider>()
                .DataMetricsProvider<SqlDataMetricsProvider>(dmp => 
                    dmp.AddMetric(Metrics.TotalStorageBytes, $"SELECT SUM(bytes) FROM system.parts WHERE active AND table = '{table.Name}'"))
                .ProgressReporter<ImportProgressReporter>()
                .Build();

        public IQueryExecutorFactory CreateQueryExecutorFactory(Table table, Query query) =>
            new SqlQueryExecutorFactory<ClickHouseConnection>(this, table, query, _environment)
                .Customize<ISqlQueryBuilder, ClickHouseQueryBuilder>()
                .Customize<ISqlParameterAdapter, ClickHouseParameterAdapter>();

        public IQueryExecutorFactory CreateRawQueryExecutorFactory(RawQuery query) =>
            new SqlRawQueryExecutorFactory<ClickHouseConnection>(this, query, _environment)
                .Customize<ISqlParameterAdapter, ClickHouseParameterAdapter>();

        public IQueryExecutorFactory CreateInsertExecutorFactory(Table table, IDataSource source, int batchSize) =>
            new SqlInsertExecutorFactory<ClickHouseConnection>(this, table, source, batchSize, _environment)
                .Customize<ISqlParameterAdapter, ClickHouseParameterAdapter>();

        public void ExecuteScript(string script) => throw new InputArgumentException("Custom scripts are not supported for ClickHouse");
    }
}
