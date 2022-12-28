using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.Model;
using System.Data;

namespace DatabaseBenchmark.Databases.Sql
{
    public class SqlDataMetricsProvider : IDataMetricsProvider
    {
        private readonly IDbConnection _connection;
        private readonly Table _table;
        private readonly Dictionary<string, string> _metricDefinitions = new();

        public SqlDataMetricsProvider(IDbConnection connection, Table table)
        {
            _connection = connection;
            _table = table;
        }

        public long GetRowCount()
        {
            using var command = _connection.CreateCommand();
            command.CommandText = $"SELECT COUNT(1) FROM {_table.Name}";
            return Convert.ToInt64(command.ExecuteScalar());
        }

        public IDictionary<string, double> GetMetrics()
        {
            Dictionary<string, double> metrics = new();

            foreach (var definition in _metricDefinitions)
            {
                using var command = _connection.CreateCommand();
                command.CommandText = definition.Value;
                var value = Convert.ToDouble(command.ExecuteScalar());
                metrics.Add(definition.Key, value);
            }

            return metrics;
        }

        public SqlDataMetricsProvider AddMetric(string name, string query)
        {
            _metricDefinitions.Add(name, query);
            return this;
        }
    }
}
