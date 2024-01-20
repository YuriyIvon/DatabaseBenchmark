using DatabaseBenchmark.Common;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Model;
using MySqlConnector;
using System.Data;

namespace DatabaseBenchmark.Databases.MySql
{
    public class MySqlParameterAdapter : ISqlParameterAdapter
    {
        public void Populate(SqlQueryParameter source, IDbDataParameter target)
        {
            target.ParameterName = source.Name;
            target.Value = source.Value ?? DBNull.Value;

            var mySqlTarget = (MySqlParameter)target;
            mySqlTarget.MySqlDbType = source.Type switch
            {
                ColumnType.Boolean => MySqlDbType.Bool,
                ColumnType.Integer => MySqlDbType.Int32,
                ColumnType.Long => MySqlDbType.Int64,
                ColumnType.Double => MySqlDbType.Double,
                ColumnType.DateTime => MySqlDbType.Timestamp,
                ColumnType.Guid => MySqlDbType.Guid,
                ColumnType.String => MySqlDbType.VarChar,
                ColumnType.Text => MySqlDbType.Text,
                _ => throw new InputArgumentException($"Parameter type {source.Type} is not supported")
            };
        }
    }
}
