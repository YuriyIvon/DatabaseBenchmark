using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using Snowflake.Data.Client;

namespace DatabaseBenchmark.Databases.Snowflake
{
    public class SnowflakeDatabase : IDatabase
    {
        private const int DefaultImportBatchSize = 10000;

        private readonly IExecutionEnvironment _environment;

        public string ConnectionString { get; }

        public SnowflakeDatabase(string connectionString, IExecutionEnvironment environment)
        {
            ConnectionString = connectionString;
            _environment = environment;

            SnowflakeDbConnectionPool.SetPooling(false);
        }

        public void CreateTable(Table table, bool dropExisting)
        {
            using var connection = new SnowflakeDbConnection
            {
                ConnectionString = ConnectionString
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

        public IDataImporter CreateDataImporter(Table table, IDataSource source, int batchSize) =>
            new SqlDataImporterBuilder(table, source, batchSize, DefaultImportBatchSize)
                .Connection<SnowflakeDbConnection>(ConnectionString)
                .ParametersBuilder(() => new SqlParametersBuilder(':'))
                .ParameterAdapter<SnowflakeParameterAdapter>()
                .TransactionProvider<SqlTransactionProvider>()
                .DataMetricsProvider<SqlDataMetricsProvider>()
                .ProgressReporter<ImportProgressReporter>()
                .Environment(_environment)
                .Build();

        public IQueryExecutorFactory CreateQueryExecutorFactory(Table table, Query query) =>
             new SqlQueryExecutorFactory<SnowflakeDbConnection>(this, table, query, _environment)
                .Customize<ISqlQueryBuilder, SnowflakeQueryBuilder>()
                .Customize<ISqlParametersBuilder>(() => new SqlParametersBuilder(':'))
                .Customize<ISqlParameterAdapter, SnowflakeParameterAdapter>();

        public IQueryExecutorFactory CreateRawQueryExecutorFactory(RawQuery query) =>
            new SqlRawQueryExecutorFactory<SnowflakeDbConnection>(this, query, _environment)
                .Customize<ISqlParametersBuilder>(() => new SqlParametersBuilder(':'))
                .Customize<ISqlParameterAdapter, SnowflakeParameterAdapter>();

        public IQueryExecutorFactory CreateInsertExecutorFactory(Table table, IDataSource source, int batchSize) =>
            new SqlInsertExecutorFactory<SnowflakeDbConnection>(this, table, source, batchSize, _environment)
                .Customize<ISqlParametersBuilder>(() => new SqlParametersBuilder(':'))
                .Customize<ISqlParameterAdapter, SnowflakeParameterAdapter>();

        public void ExecuteScript(string script)
        {
            using var connection = new SnowflakeDbConnection(ConnectionString);
            connection.ExecuteScript(script);
        }
    }
}
