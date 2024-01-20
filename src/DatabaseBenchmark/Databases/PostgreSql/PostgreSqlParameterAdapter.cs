using DatabaseBenchmark.Common;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Model;
using Npgsql;
using NpgsqlTypes;
using System.Data;

namespace DatabaseBenchmark.Databases.PostgreSql
{
    public class PostgreSqlParameterAdapter : ISqlParameterAdapter
    {
        public virtual void Populate(SqlQueryParameter source, IDbDataParameter target)
        {
            target.ParameterName = source.Name;
            target.Value = source.Value ?? DBNull.Value;

            var postgresTarget = (NpgsqlParameter)target;
            postgresTarget.NpgsqlDbType = source.Type switch
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
                _ => throw new InputArgumentException($"Parameter type {source.Type} is not supported")
            };
        }
    }
}
