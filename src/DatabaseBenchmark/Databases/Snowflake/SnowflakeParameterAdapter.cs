using DatabaseBenchmark.Common;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Model;
using Snowflake.Data.Client;
using Snowflake.Data.Core;
using System.Data;

namespace DatabaseBenchmark.Databases.Snowflake
{
    public class SnowflakeParameterAdapter : ISqlParameterAdapter
    {
        public void Populate(SqlQueryParameter source, IDbDataParameter target)
        {
            target.ParameterName = source.Name;
            target.Value = source.Value ?? DBNull.Value;

            var snowflakeTarget = (SnowflakeDbParameter)target;
            snowflakeTarget.SFDataType = source.Type switch
            {
                ColumnType.Boolean => SFDataType.BOOLEAN,
                ColumnType.Integer => SFDataType.FIXED,
                ColumnType.Long => SFDataType.FIXED,
                ColumnType.Double => SFDataType.REAL,
                ColumnType.DateTime => SFDataType.TIMESTAMP_NTZ,
                ColumnType.Guid => SFDataType.TEXT,
                ColumnType.String => SFDataType.TEXT,
                ColumnType.Text => SFDataType.TEXT,
                _ => throw new InputArgumentException($"Parameter type {source.Type} is not supported")
            };
        }
    }
}
