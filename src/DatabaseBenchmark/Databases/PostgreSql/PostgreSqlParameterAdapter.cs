using DatabaseBenchmark.Databases.Sql.Interfaces;
using Npgsql;
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
            postgresTarget.NpgsqlDbType = PostgreSqlDatabaseUtils.GetNativeColumnType(source.Type, source.Array);
        }
    }
}
