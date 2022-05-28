using DatabaseBenchmark.Common;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Model;
using System.Text;

namespace DatabaseBenchmark.Databases.MonetDb
{
    public class MonetDbTableBuilder : SqlTableBuilder
    {
        protected override string BuildColumnType(Column column)
        {
            if (column.DatabaseGenerated && column.Nullable)
            {
                throw new InputArgumentException("Database-generated columns must not be nullable");
            }

            var columnType = new StringBuilder(column.Type switch
            {
                ColumnType.Boolean => "boolean",
                ColumnType.Guid => "uuid",
                ColumnType.Integer => "integer",
                ColumnType.Long => "bigint",
                ColumnType.Double => "double precision",
                ColumnType.String => "varchar(1000)",
                ColumnType.Text => "text",
                ColumnType.DateTime => "timestamp",
                _ => throw new InputArgumentException($"Unknown column type \"{column.Type}\"")
            });

            if (!column.Nullable)
            {
                columnType.Append(" NOT NULL");
            }

            if (column.DatabaseGenerated)
            {
                var suffix = column.Type switch
                {
                    ColumnType.Guid => " DEFAULT (uuid())",
                    ColumnType.Integer => " AUTO_INCREMENT",
                    ColumnType.Long => " AUTO_INCREMENT",
                    _ => throw new InputArgumentException($"Column type \"{column.Type}\" can't be database-generated")
                };

                columnType.Append(suffix);
            }

            return columnType.ToString();
        }
    }
}
