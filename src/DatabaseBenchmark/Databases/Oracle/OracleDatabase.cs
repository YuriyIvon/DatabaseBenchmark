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

        private readonly IExecutionEnvironment _environment;

        public string ConnectionString { get; }

        public OracleDatabase(string connectionString, IExecutionEnvironment environment)
        {
            ConnectionString = connectionString;
            _environment = environment;
        }

        public void CreateTable(Table table, bool dropExisting)
        {
            using var connection = new OracleConnection(ConnectionString);
            connection.Open();

            if (dropExisting)
            {
                connection.DropTableIfExists(table.Name);
            }

            var tableBuilder = new OracleTableBuilder();
            var commandText = tableBuilder.Build(table);
            var command = new OracleCommand(commandText, connection);

            _environment.TraceCommand(command.CommandText);

            command.ExecuteNonQuery();
        }

        public IDataImporter CreateDataImporter(Table table, IDataSource source, int batchSize) =>
            new SqlDataImporterBuilder(table, source, batchSize, DefaultImportBatchSize)
                .Connection<OracleConnection>(ConnectionString)
                .ParametersBuilder(() => new SqlParametersBuilder(':'))
                .ParameterAdapter<OracleParameterAdapter>()
                .InsertBuilder<ISqlQueryBuilder, OracleInsertBuilder>()
                .TransactionProvider<SqlTransactionProvider>()
                .DataMetricsProvider<SqlDataMetricsProvider>()
                .ProgressReporter<ImportProgressReporter>()
                .Environment(_environment)
                .Build();

        public IQueryExecutorFactory CreateQueryExecutorFactory(Table table, Query query) =>
            new SqlQueryExecutorFactory<OracleConnection>(this, table, query, _environment)
                .Customize<ISqlParametersBuilder>(() => new SqlParametersBuilder(':'))
                .Customize<ISqlParameterAdapter, OracleParameterAdapter>();

        public IQueryExecutorFactory CreateRawQueryExecutorFactory(RawQuery query) =>
            new SqlRawQueryExecutorFactory<OracleConnection>(this, query, _environment)
                .Customize<ISqlParametersBuilder>(() => new SqlParametersBuilder(':'))
                .Customize<ISqlParameterAdapter, OracleParameterAdapter>();

        public IQueryExecutorFactory CreateInsertExecutorFactory(Table table, IDataSource source, int batchSize) =>
            new SqlInsertExecutorFactory<OracleConnection>(this, table, source, batchSize, _environment)
                .Customize<ISqlParametersBuilder>(() => new SqlParametersBuilder(':'))
                .Customize<ISqlQueryBuilder, OracleInsertBuilder>()
                .Customize<ISqlParameterAdapter, OracleParameterAdapter>();

        public void ExecuteScript(string script)
        {
            using var connection = new OracleConnection(ConnectionString);
            connection.ExecuteScript(script);
        }
    }
}
