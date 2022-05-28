using DatabaseBenchmark.Core.Interfaces;
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

        public List<object> GetDistinctValues(string tableName, string columnName)
        {
            var command = _connection.CreateCommand();
            command.CommandText = $"SELECT DISTINCT {columnName} FROM {tableName}";

            _environment.TraceCommand(command);

            return command.ReadAsList<object>();
        }
    }
}
