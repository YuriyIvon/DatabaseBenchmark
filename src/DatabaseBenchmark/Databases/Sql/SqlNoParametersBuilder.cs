using DatabaseBenchmark.Common;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Databases.Sql
{
    public class SqlNoParametersBuilder : ISqlParametersBuilder
    {
        public IEnumerable<SqlQueryParameter> Parameters { get; } = Enumerable.Empty<SqlQueryParameter>();

        public virtual string Append(object value, ColumnType type, bool array) =>
            value switch
            {
                IEnumerable<object> => throw new InputArgumentException("Array literals are not supported"),
                null => "NULL",
                bool boolValue => boolValue.ToString().ToLower(), //Different databases may accept different Boolean format
                int intValue => intValue.ToString(),
                long longValue => longValue.ToString(),
                double doubleValue => doubleValue.ToString(),
                DateTime dateTimeValue => Quote(dateTimeValue.ToString("o")),
                Guid guidValue => Quote(guidValue.ToString()), //Different databases may accept different UUID format
                _ => Quote(value.ToString().Replace("'", "''"))
            };

        public void Reset()
        {
        }

        private static string Quote(string value) => "'" + value + "'";
    }
}
