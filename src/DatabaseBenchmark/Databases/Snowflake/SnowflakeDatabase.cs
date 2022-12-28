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

        public IDataImporter CreateDataImporter(Table table, IDataSource source, int batchSize) =>
            new SqlDataImporterBuilder(table, source, batchSize, DefaultImportBatchSize)
                .Connection<SnowflakeDbConnection>(_connectionString)
                .ParametersBuilder(() => new SqlParametersBuilder(':'))
                .ParameterAdapter<SnowflakeParameterAdapter>()
                .TransactionProvider<SqlTransactionProvider>()
                .DataMetricsProvider<SqlDataMetricsProvider>()
                .ProgressReporter<ImportProgressReporter>()
                .Environment(_environment)
                .Build();

        public IQueryExecutorFactory CreateQueryExecutorFactory(Table table, Query query) =>
             new SqlQueryExecutorFactory<SnowflakeDbConnection>(_connectionString, table, query, _environment)
                .Customize<ISqlQueryBuilder, SnowflakeQueryBuilder>()
                .Customize<ISqlParametersBuilder>(() => new SqlParametersBuilder(':'))
                .Customize<ISqlParameterAdapter, SnowflakeParameterAdapter>();

        public IQueryExecutorFactory CreateRawQueryExecutorFactory(RawQuery query) =>
            new SqlRawQueryExecutorFactory<SnowflakeDbConnection>(_connectionString, query, _environment)
                .Customize<ISqlParametersBuilder>(() => new SqlParametersBuilder(':'))
                .Customize<ISqlParameterAdapter, SnowflakeParameterAdapter>();

        public IQueryExecutorFactory CreateInsertExecutorFactory(Table table, IDataSource source, int batchSize) =>
            new SqlInsertExecutorFactory<SnowflakeDbConnection>(_connectionString, table, source, batchSize, _environment)
                .Customize<ISqlParametersBuilder>(() => new SqlParametersBuilder(':'))
                .Customize<ISqlParameterAdapter, SnowflakeParameterAdapter>();
    }
}
