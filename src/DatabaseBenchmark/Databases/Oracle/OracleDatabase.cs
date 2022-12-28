using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using Oracle.ManagedDataAccess.Client;

namespace DatabaseBenchmark.Databases.Oracle
{
    public class OracleDatabase : IDatabase
    {
        private const int DefaultImportBatchSize = 10;

        private readonly string _connectionString;
        private readonly IExecutionEnvironment _environment;

        public OracleDatabase(string connectionString, IExecutionEnvironment environment)
        {
            _connectionString = connectionString;
            _environment = environment;
        }

        public void CreateTable(Table table, bool dropExisting)
        {
            using var connection = new OracleConnection(_connectionString);
            connection.Open();

            if (dropExisting)
            {
                connection.DropTableIfExists(table.Name);
            }

            var tableBuilder = new OracleTableBuilder();
            var commandText = tableBuilder.Build(table);
            var command = new OracleCommand(commandText, connection);

            _environment.TraceCommand(command);

            command.ExecuteNonQuery();
        }

        public IDataImporter CreateDataImporter(Table table, IDataSource source, int batchSize) =>
            new SqlDataImporterBuilder(table, source, batchSize, DefaultImportBatchSize)
                .Connection<OracleConnection>(_connectionString)
                .ParametersBuilder(() => new SqlParametersBuilder(':'))
                .ParameterAdapter<OracleParameterAdapter>()
                .InsertBuilder<ISqlQueryBuilder, OracleInsertBuilder>()
                .TransactionProvider<SqlTransactionProvider>()
                .DataMetricsProvider<SqlDataMetricsProvider>()
                .ProgressReporter<ImportProgressReporter>()
                .Environment(_environment)
                .Build();

        public IQueryExecutorFactory CreateQueryExecutorFactory(Table table, Query query) =>
            new SqlQueryExecutorFactory<OracleConnection>(_connectionString, table, query, _environment)
                .Customize<ISqlParametersBuilder>(() => new SqlParametersBuilder(':'))
                .Customize<ISqlParameterAdapter, OracleParameterAdapter>();

        public IQueryExecutorFactory CreateRawQueryExecutorFactory(RawQuery query) =>
            new SqlRawQueryExecutorFactory<OracleConnection>(_connectionString, query, _environment)
                .Customize<ISqlParametersBuilder>(() => new SqlParametersBuilder(':'))
                .Customize<ISqlParameterAdapter, OracleParameterAdapter>();

        public IQueryExecutorFactory CreateInsertExecutorFactory(Table table, IDataSource source, int batchSize) =>
            new SqlInsertExecutorFactory<OracleConnection>(_connectionString, table, source, batchSize, _environment)
                .Customize<ISqlParametersBuilder>(() => new SqlParametersBuilder(':'))
                .Customize<ISqlQueryBuilder, OracleInsertBuilder>()
                .Customize<ISqlParameterAdapter, OracleParameterAdapter>();
    }
}
