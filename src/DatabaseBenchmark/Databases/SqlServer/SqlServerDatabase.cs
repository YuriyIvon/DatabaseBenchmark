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
        private readonly IExecutionEnvironment _environment;

        public string ConnectionString { get; }

        public SqlServerDatabase(string connectionString, IExecutionEnvironment environment)
        {
            ConnectionString = connectionString;
            _environment = environment;
        }

        public void CreateTable(Table table, bool dropExisting)
        {
            using var connection = new SqlConnection(ConnectionString);
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
            new SqlServerDataImporter(ConnectionString, table, source, batchSize, _environment);

        public IQueryExecutorFactory CreateQueryExecutorFactory(Table table, Query query) =>
            new SqlQueryExecutorFactory<SqlConnection>(this, table, query, _environment)
                .Customize<ISqlParameterAdapter, SqlServerParameterAdapter>();

        public IQueryExecutorFactory CreateRawQueryExecutorFactory(RawQuery query) =>
            new SqlRawQueryExecutorFactory<SqlConnection>(this, query, _environment)
                .Customize<ISqlParameterAdapter, SqlServerParameterAdapter>();

        public IQueryExecutorFactory CreateInsertExecutorFactory(Table table, IDataSource source, int batchSize) =>
            new SqlInsertExecutorFactory<SqlConnection>(this, table, source, batchSize, _environment)
                .Customize<ISqlParameterAdapter, SqlServerParameterAdapter>();

        public void ExecuteScript(string script)
        {
            using var connection = new SqlConnection(ConnectionString);
            connection.ExecuteScript(script);
        }
    }
}
