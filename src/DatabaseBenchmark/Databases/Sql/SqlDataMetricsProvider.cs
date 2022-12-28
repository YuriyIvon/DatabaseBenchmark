using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Model;
using System.Data;

namespace DatabaseBenchmark.Databases.Sql
{
    public class SqlDataMetricsProvider : IDataMetricsProvider
    {
        private readonly IDbConnection _connection;
        private readonly Table _table;

        public SqlDataMetricsProvider(IDbConnection connection, Table table)
        {
            _connection = connection;
            _table = table;
        }

        public long GetRowCount()
        {
            var command = _connection.CreateCommand();
            command.CommandText = $"SELECT COUNT(1) FROM {_table.Name}";
            return Convert.ToInt64(command.ExecuteScalar());
        }
    }
}
