using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Model;
using System.Data;

namespace DatabaseBenchmark.Databases.PostgreSql
{
    public class PostgreSqlDistinctValuesProvider : IDistinctValuesProvider
    {
        private readonly IDbConnection _connection;
        private readonly IExecutionEnvironment _environment;

        public PostgreSqlDistinctValuesProvider(
            IDbConnection connection,
            IExecutionEnvironment environment)
        {
            _environment = environment;
            _connection = connection;
        }

        public object[] GetDistinctValues(string tableName, IValueDefinition column, bool unfoldArray)
        {
            var command = _connection.CreateCommand();
            command.CommandText = unfoldArray
                ? $"SELECT DISTINCT unnest({column.Name}) FROM {tableName}"
                : $"SELECT DISTINCT {column.Name} FROM {tableName}";

            _environment.TraceCommand(command.CommandText);

            return command.ReadAsArray<object>();
        }
    }
}
