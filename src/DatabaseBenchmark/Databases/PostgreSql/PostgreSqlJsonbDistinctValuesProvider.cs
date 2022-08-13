using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using System.Data;

namespace DatabaseBenchmark.Databases.PostgreSql
{
    public class PostgreSqlJsonbDistinctValuesProvider : IDistinctValuesProvider
    {
        private readonly IDbConnection _connection;
        private readonly IExecutionEnvironment _environment;

        public PostgreSqlJsonbDistinctValuesProvider(
            IDbConnection connection,
            IExecutionEnvironment environment)
        {
            _environment = environment;
            _connection = connection;
        }

        public object[] GetDistinctValues(string tableName, string columnName)
        {
            var command = _connection.CreateCommand();
            command.CommandText = $"SELECT DISTINCT {PostgreSqlJsonbConstants.JsonbColumnName}->>'{columnName}' FROM {tableName}";
            _environment.TraceCommand(command);
            return command.ReadAsArray<object>();
        }
    }
}
