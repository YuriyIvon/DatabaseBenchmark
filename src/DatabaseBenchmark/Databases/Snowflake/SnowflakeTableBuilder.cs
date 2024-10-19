using DatabaseBenchmark.Common;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Model;
using System.Text;

namespace DatabaseBenchmark.Databases.Snowflake
{
    public class SnowflakeTableBuilder : SqlTableBuilder
    {
        protected override string BuildColumnType(Column column)
        {
            if (column.DatabaseGenerated && column.Nullable)
            {
                throw new InputArgumentException("Database-generated columns must not be nullable");
            }

            if (column.Array)
            {
                throw new InputArgumentException("Snowflake array columns are not supported yet");
            }

            var columnType = new StringBuilder(column.Type switch
            {
                ColumnType.Boolean => "BOOLEAN",
                ColumnType.Guid => "VARCHAR(36)",
                ColumnType.Integer => "INTEGER",
                ColumnType.Long => "INTEGER",
                ColumnType.Double => "FLOAT",
                ColumnType.String => "VARCHAR(1000)",
                ColumnType.Text => "TEXT",
                ColumnType.DateTime => "DATETIME",
                _ => throw new InputArgumentException($"Unknown column type \"{column.Type}\"")
            });

            if (column.DatabaseGenerated)
            {
                var suffix = column.Type switch
                {
                    ColumnType.Guid => " DEFAULT UUID_STRING()",
                    ColumnType.Integer => " AUTOINCREMENT",
                    ColumnType.Long => " AUTOINCREMENT",
                    _ => throw new InputArgumentException($"Column type \"{column.Type}\" can't be database-generated")
                };

                columnType.Append(suffix);
            }

            if (!column.Nullable)
            {
                columnType.Append(" NOT NULL");
            }

            return columnType.ToString();
        }
    }
}
