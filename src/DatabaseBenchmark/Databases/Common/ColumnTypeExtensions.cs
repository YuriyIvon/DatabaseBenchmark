using DatabaseBenchmark.Common;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Databases.Common
{
    public static class ColumnTypeExtensions
    {
        public static Type GetNativeType(this ColumnType columnType) =>
            columnType switch
            {
                ColumnType.Boolean => typeof(bool),
                ColumnType.Double => typeof(double),
                ColumnType.Integer => typeof(int),
                ColumnType.Text => typeof(string),
                ColumnType.String => typeof(string),
                ColumnType.DateTime => typeof(DateTime),
                ColumnType.Guid => typeof(Guid),
                _ => throw new InputArgumentException($"Unknown column type \"{columnType}\"")
            };
    }
}
