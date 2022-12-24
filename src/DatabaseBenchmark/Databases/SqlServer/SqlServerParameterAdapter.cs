using DatabaseBenchmark.Common;
using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Model;
using System.Data;

namespace DatabaseBenchmark.Databases.SqlServer
{
    public class SqlServerParameterAdapter : ISqlParameterAdapter
    {
        public void Populate(SqlQueryParameter source, IDbDataParameter target)
        {
            target.ParameterName = source.Name;
            target.Value = source.Value ?? DBNull.Value;
            target.DbType = source.Type switch
            {
                ColumnType.Boolean => DbType.Boolean,
                ColumnType.Integer => DbType.Int32,
                ColumnType.Long => DbType.Int64,
                ColumnType.Double => DbType.Double,
                ColumnType.DateTime => DbType.DateTime2,
                ColumnType.Guid => DbType.Guid,
                ColumnType.String => DbType.String,
                ColumnType.Text => DbType.String,
                _ => throw new InputArgumentException($"Parameter type {source.Type} is not supported")
            };
        }
    }
}
