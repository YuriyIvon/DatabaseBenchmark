using DatabaseBenchmark.Common;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Databases.Common
{
    public static class ColumnTypeExtensions
    {
        public static Type GetNativeType(this Column column) =>
            column.Type switch
            {
                ColumnType.Boolean when column.Nullable => typeof(bool?),
                ColumnType.Boolean => typeof(bool),
                ColumnType.Double when column.Nullable => typeof(double?),
                ColumnType.Double => typeof(double),
                ColumnType.Integer when column.Nullable => typeof(int?),
                ColumnType.Integer => typeof(int),
                ColumnType.Text => typeof(string),
                ColumnType.String => typeof(string),
                ColumnType.DateTime when column.Nullable => typeof(DateTime?),
                ColumnType.DateTime => typeof(DateTime),
                ColumnType.Guid when column.Nullable => typeof(Guid?),
                ColumnType.Guid => typeof(Guid),
                _ => throw new InputArgumentException($"Unknown column type \"{column.Type}\"")
            };
    }
}
