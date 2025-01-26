using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Model;
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

        public object[] GetDistinctValues(string tableName, IValueDefinition column, bool unfoldArray)
        {
            var columnReference = $"{PostgreSqlJsonbConstants.JsonbColumnName}->>'{column.Name}'";

            var command = _connection.CreateCommand();
            command.CommandText = unfoldArray
                ? $"SELECT DISTINCT {PostgreSqlDatabaseUtils.CastExpression("value->>0", column.Type)} FROM {tableName}, jsonb_array_elements({PostgreSqlJsonbConstants.JsonbColumnName}->'{column.Name}')"
                : $"SELECT DISTINCT {PostgreSqlDatabaseUtils.CastExpression(columnReference, column.Type)} FROM {tableName}";

            _environment.TraceCommand(command.CommandText);
            return command.ReadAsArray<object>();
        }
    }
}
