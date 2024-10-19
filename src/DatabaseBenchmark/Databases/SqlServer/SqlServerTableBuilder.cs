using DatabaseBenchmark.Common;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Model;
using System.Text;

namespace DatabaseBenchmark.Databases.SqlServer
{
    public class SqlServerTableBuilder : SqlTableBuilder
    {
        protected override string BuildColumnType(Column column)
        {
            if (column.DatabaseGenerated && column.Nullable)
            {
                throw new InputArgumentException("Database-generated columns must not be nullable");
            }

            if (column.Array)
            {
                throw new InputArgumentException("MS SQL doesn't support array columns");
            }

            var columnType = new StringBuilder(column.Type switch
            {
                ColumnType.Boolean => "bit",
                ColumnType.Guid => "uniqueidentifier",
                ColumnType.Integer => "int",
                ColumnType.Long => "bigint",
                ColumnType.Double => "float",
                ColumnType.String => "nvarchar(1000)",
                ColumnType.Text => "nvarchar(max)",
                ColumnType.DateTime => "datetime2",
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
                    ColumnType.Guid => " DEFAULT NEWSEQUENTIALID()",
                    ColumnType.Integer => " IDENTITY",
                    ColumnType.Long => " IDENTITY",
                    _ => throw new InputArgumentException($"Column type \"{column.Type}\" can't be database-generated")
                };

                columnType.Append(suffix);
            }

            return columnType.ToString();
        }
    }
}
