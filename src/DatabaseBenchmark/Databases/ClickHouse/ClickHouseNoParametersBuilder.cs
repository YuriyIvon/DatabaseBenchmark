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
                IEnumerable<object> arrayValue => $"[{string.Join(", ", arrayValue.Select(x => Append(x, type, false)))}]",
                _ => base.Append(value, type, array)
            };
    }
}
