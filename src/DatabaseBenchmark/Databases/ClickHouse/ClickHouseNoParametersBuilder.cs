using DatabaseBenchmark.Common;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Databases.ClickHouse
{
    public class ClickHouseNoParametersBuilder : SqlNoParametersBuilder
    {
        public override string Append(object value, ColumnType type, bool array) =>
            value switch
            {
                DateTime dateTimeValue => Quote(dateTimeValue.ToSortableString()), //TODO: Make format customizable
                IEnumerable<object> arrayValue => $"[{string.Join(", ", arrayValue.Select(x => Append(x, type, false)))}]",
                Array arrayValue => $"[{string.Join(", ", arrayValue.Cast<object>().Select(x => Append(x, type, false)))}]",
                _ => base.Append(value, type, array)
            };

        private static string Quote(string value) => "'" + value + "'";
    }
}
