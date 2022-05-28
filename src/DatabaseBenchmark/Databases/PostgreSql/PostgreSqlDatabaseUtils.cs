using DatabaseBenchmark.Common;
using DatabaseBenchmark.Model;
using Npgsql;

namespace DatabaseBenchmark.Databases.PostgreSql
{
    public class PostgreSqlDatabaseUtils
    {
        public static long GetTableSize(NpgsqlConnection connection, string tableName)
        {
            var command = new NpgsqlCommand($"SELECT pg_total_relation_size('{tableName}')", connection);
            return (long)command.ExecuteScalar();
        }

        public static long GetRowCount(NpgsqlConnection connection, string tableName)
        {
            var command = new NpgsqlCommand($"SELECT COUNT(1) FROM {tableName}", connection);
            return (long)command.ExecuteScalar();
        }

        public static string BuildColumnType(Column column)
        {
            if (column.DatabaseGenerated && column.Nullable)
            {
                throw new InputArgumentException("Database-generated columns must not be nullable");
            }

            string baseType = column.DatabaseGenerated
                ? column.Type switch
                {
                    ColumnType.Integer => "serial",
                    ColumnType.Long => "bigserial",
                    _ => throw new InputArgumentException($"Column type \"{column.Type}\" can't be database-generated")
                }
                : column.Type switch
                {
                    ColumnType.Boolean => "boolean",
                    ColumnType.Guid => "uuid",
                    ColumnType.Integer => "integer",
                    ColumnType.Long => "bigint",
                    ColumnType.Double => "double precision",
                    ColumnType.String => "varchar(1000)",
                    ColumnType.Text => "varchar",
                    ColumnType.DateTime => "timestamp",
                    _ => throw new InputArgumentException($"Unknown column type \"{column.Type}\"")
                };

            return column.Nullable ? baseType : $"{baseType} NOT NULL";
        }
    }
}
