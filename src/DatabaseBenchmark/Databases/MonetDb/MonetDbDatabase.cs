using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using MonetDb.Mapi;

namespace DatabaseBenchmark.Databases.MonetDb
{
    public class MonetDbDatabase : IDatabase
    {
        private const int DefaultImportBatchSize = 100;

        private readonly string _connectionString;
        private readonly IExecutionEnvironment _environment;

        public MonetDbDatabase(string connectionString, IExecutionEnvironment environment)
        {
            _connectionString = connectionString;
            _environment = environment;
        }

        public void CreateTable(Table table, bool dropExisting)
        {
            using var connection = new MonetDbConnection(_connectionString);
            connection.Open();

            if (dropExisting)
            {
                connection.DropTableIfExists(table.Name);
            }

            var tableBuilder = new MonetDbTableBuilder();
            var commandText = tableBuilder.Build(table);
            var command = new MonetDbCommand(commandText, connection);

            _environment.TraceCommand(command);

            command.ExecuteNonQuery();
        }

        public IDataImporter CreateDataImporter(Table table, IDataSource source, int batchSize) =>
            new SqlDataImporterBuilder(table, source, batchSize, DefaultImportBatchSize)
                .Connection<MonetDbConnection>(_connectionString)
                .ParametersBuilder<SqlNoParametersBuilder>()
                .ParameterAdapter<MonetDbParameterAdapter>()
                .TransactionProvider<MonetDbTransactionProvider>()
                .DataMetricsProvider<SqlDataMetricsProvider>(dmp =>
                    dmp.AddMetric(Metrics.TotalStorageBytes, $"SELECT SUM(columnsize) + SUM(heapsize) + SUM(hashes) + SUM(imprints) + SUM(orderidx) FROM storage() WHERE table = '{table.Name.ToLower()}'"))
                .ProgressReporter<ImportProgressReporter>()
                .Environment(_environment)
                .Build();

        public IQueryExecutorFactory CreateQueryExecutorFactory(Table table, Query query) =>
            new SqlQueryExecutorFactory<MonetDbConnection>(_connectionString, table, query, _environment)
                .Customize<ISqlQueryBuilder, MonetDbQueryBuilder>()
                .Customize<ISqlParameterAdapter, MonetDbParameterAdapter>();

        public IQueryExecutorFactory CreateRawQueryExecutorFactory(RawQuery query) =>
            new SqlRawQueryExecutorFactory<MonetDbConnection>(_connectionString, query, _environment)
                .Customize<ISqlParameterAdapter, MonetDbParameterAdapter>();

        public IQueryExecutorFactory CreateInsertExecutorFactory(Table table, IDataSource source, int batchSize) =>
            new SqlInsertExecutorFactory<MonetDbConnection>(_connectionString, table, source, batchSize, _environment)
                .Customize<ISqlParametersBuilder, SqlNoParametersBuilder>()
                .Customize<ISqlParameterAdapter, MonetDbParameterAdapter>();
    }
}
