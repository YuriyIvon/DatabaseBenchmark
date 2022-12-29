using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Databases.Sql
{
    public class SqlNoParametersBuilder : ISqlParametersBuilder
    {
        public IEnumerable<SqlQueryParameter> Parameters { get; } = Enumerable.Empty<SqlQueryParameter>();

        public string Append(object value, ColumnType type)
        {
            if (value != null)
            {
                var stringValue = value is DateTime dateTimeValue ? dateTimeValue.ToString("o") : value.ToString();

                return (value is bool || value is int || value is long || value is double)
                    ? stringValue : $"'{stringValue.Replace("'", "''")}'";
            }

            return "NULL";
        }

        public void Reset()
        {
        }
    }
}
