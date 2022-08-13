using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using Npgsql;

namespace DatabaseBenchmark.Databases.PostgreSql
{
    public class PostgreSqlJsonbDistinctValuesProvider : IDistinctValuesProvider
    {
        private readonly NpgsqlConnection _connection;
        private readonly IExecutionEnvironment _environment;

        public PostgreSqlJsonbDistinctValuesProvider(
            NpgsqlConnection connection,
            IExecutionEnvironment environment)
        {
            _environment = environment;
            _connection = connection;
        }

        public object[] GetDistinctValues(string tableName, string columnName)
        {
            var command = new NpgsqlCommand($"SELECT DISTINCT {PostgreSqlJsonbConstants.JsonbColumnName}->>'{columnName}' FROM {tableName}", _connection);
            _environment.TraceCommand(command);
            return command.ReadAsArray<object>();
        }
    }
}
