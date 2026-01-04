using DatabaseBenchmark.Common;
using DatabaseBenchmark.Model;
using Npgsql;
using NpgsqlTypes;

namespace DatabaseBenchmark.Databases.PostgreSql
{
    public static class PostgreSqlDatabaseUtils
    {
        public static long GetTableSize(NpgsqlConnection connection, string tableName)
        {
            var command = new NpgsqlCommand($"SELECT pg_total_relation_size('{tableName}')", connection);
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
                    ColumnType.DateTime => "timestamp",
                    ColumnType.String => "varchar(1000)",
                    ColumnType.Text => "varchar",
                    ColumnType.Vector => column.Size != null
                        ? $"vector({column.Size})"
                        : throw new InputArgumentException("A vector column must have a specified size"),
                    _ => throw new InputArgumentException($"Unknown column type \"{column.Type}\"")
                };

            if (column.Array)
            {
                baseType += "[]";
            }

            return column.Nullable ? baseType : $"{baseType} NOT NULL";
        }

        public static NpgsqlDbType GetNativeColumnType(ColumnType columnType, bool array)
        {
            var nativeType = columnType switch
            {
                ColumnType.Boolean => NpgsqlDbType.Boolean,
                ColumnType.Integer => NpgsqlDbType.Integer,
                ColumnType.Long => NpgsqlDbType.Bigint,
                ColumnType.Double => NpgsqlDbType.Double,
                ColumnType.DateTime => NpgsqlDbType.Timestamp,
                ColumnType.Guid => NpgsqlDbType.Uuid,
                ColumnType.String => NpgsqlDbType.Varchar,
                ColumnType.Text => NpgsqlDbType.Varchar,
                ColumnType.Json => NpgsqlDbType.Jsonb,
                _ => throw new InputArgumentException($"Unknown column type \"{columnType}\"")
            };

            if (array)
            {
                nativeType |= NpgsqlDbType.Array;
            }

            return nativeType;
        }

        public static string CastExpression(string expression, ColumnType columnType)
        {
            var castType = columnType switch
            {
                ColumnType.Boolean => "boolean",
                ColumnType.Guid => "uuid",
                ColumnType.Integer => "integer",
                ColumnType.Long => "bigint",
                ColumnType.Double => "double",
                _ => null
            };

            return castType != null ? $"({expression})::{castType}" : expression;
        }
    }
}
