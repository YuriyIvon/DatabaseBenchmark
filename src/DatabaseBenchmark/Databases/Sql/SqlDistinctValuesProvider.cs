using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Model;
using System.Data;

namespace DatabaseBenchmark.Databases.Sql
{
    public class SqlDistinctValuesProvider : IDistinctValuesProvider
    {
        private readonly IDbConnection _connection;
        private readonly IExecutionEnvironment _environment;

        public SqlDistinctValuesProvider(
            IDbConnection connection,
            IExecutionEnvironment environment)
        {
            _environment = environment;
            _connection = connection;
        }

        public object[] GetDistinctValues(string tableName, IValueDefinition column, bool unfoldArray)
        {
            var command = _connection.CreateCommand();
            command.CommandText = $"SELECT DISTINCT {column.Name} FROM {tableName}";

            _environment.TraceCommand(command.CommandText);

            return command.ReadAsArray<object>();
        }
    }
}
