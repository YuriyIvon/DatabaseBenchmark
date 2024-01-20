using DatabaseBenchmark.Common;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Model;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace DatabaseBenchmark.Databases.Oracle
{
    public class OracleParameterAdapter : ISqlParameterAdapter
    {
        public void Populate(SqlQueryParameter source, IDbDataParameter target)
        {
            target.ParameterName = source.Name;

            if (source.Type == ColumnType.Boolean && source.Value != null)
            {
                target.Value = (bool)source.Value ? 1 : 0;
            }
            else if (source.Type == ColumnType.Guid && source.Value != null)
            {
                target.Value = ((Guid)source.Value).ToByteArray();
            }
            else
            {
                target.Value = source.Value ?? DBNull.Value;
            }

            var oracleTarget = (OracleParameter)target;
            oracleTarget.OracleDbType = source.Type switch
            {
                ColumnType.Boolean => OracleDbType.Int32,
                ColumnType.Integer => OracleDbType.Int32,
                ColumnType.Long => OracleDbType.Int64,
                ColumnType.Double => OracleDbType.Double,
                ColumnType.DateTime => OracleDbType.Date,
                ColumnType.Guid => OracleDbType.Raw,
                ColumnType.String => OracleDbType.Varchar2,
                ColumnType.Text => OracleDbType.Long,
                _ => throw new InputArgumentException($"Parameter type {source.Type} is not supported")
            };
        }
    }
}
