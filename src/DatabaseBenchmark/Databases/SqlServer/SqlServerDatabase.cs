using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Model;
using System.Data.SqlClient;

namespace DatabaseBenchmark.Databases.SqlServer
{
    public class SqlServerDatabase : IDatabase
    {
        private readonly string _connectionString;
        private readonly IExecutionEnvironment _environment;

        public SqlServerDatabase(string connectionString, IExecutionEnvironment environment)
        {
            _connectionString = connectionString;
            _environment = environment;
        }

        public void CreateTable(Table table, bool dropExisting)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            if (dropExisting)
            {
                connection.DropTableIfExists(table.Name);
            }

            var tableBuilder = new SqlServerTableBuilder();
            var commandText = tableBuilder.Build(table);
            var command = new SqlCommand(commandText, connection);

            _environment.TraceCommand(command);

            command.ExecuteNonQuery();
        }

        public IDataImporter CreateDataImporter(Table table, IDataSource source, int batchSize) =>
            new SqlServerDataImporter(_connectionString, table, source, batchSize, _environment);

        public IQueryExecutorFactory CreateQueryExecutorFactory(Table table, Query query) =>
            new SqlQueryExecutorFactory<SqlConnection>(_connectionString, table, query, _environment)
                .Customize<ISqlParameterAdapter, SqlServerParameterAdapter>();

        public IQueryExecutorFactory CreateRawQueryExecutorFactory(RawQuery query) =>
            new SqlRawQueryExecutorFactory<SqlConnection>(_connectionString, query, _environment)
                .Customize<ISqlParameterAdapter, SqlServerParameterAdapter>();

        public IQueryExecutorFactory CreateInsertExecutorFactory(Table table, IDataSource source, int batchSize) =>
            new SqlInsertExecutorFactory<SqlConnection>(_connectionString, table, source, batchSize, _environment)
                .Customize<ISqlParameterAdapter, SqlServerParameterAdapter>();
    }
}
