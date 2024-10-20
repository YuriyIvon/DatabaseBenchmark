using DatabaseBenchmark.Common;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Model;
using Octonica.ClickHouseClient;
using System.Data;

namespace DatabaseBenchmark.Databases.ClickHouse
{
    public class ClickHouseParameterAdapter : ISqlParameterAdapter
    {
        public void Populate(SqlQueryParameter source, IDbDataParameter target)
        {
            target.ParameterName = source.Name;

            if (source.Value?.GetType() == typeof(long) && source.Type == ColumnType.Integer)
            {
                target.Value = Convert.ToInt32(source.Value);
            }
            else
            {
                target.Value = source.Value ?? DBNull.Value;
            }

            var clickHouseTarget = (ClickHouseParameter)target;
            clickHouseTarget.ClickHouseDbType = source.Type switch
            {
                ColumnType.Boolean => ClickHouseDbType.Boolean,
                ColumnType.Integer => ClickHouseDbType.Int32,
                ColumnType.Long => ClickHouseDbType.Int64,
                ColumnType.Double => ClickHouseDbType.Double,
                ColumnType.DateTime => ClickHouseDbType.DateTime,
                ColumnType.Guid => ClickHouseDbType.Guid,
                ColumnType.String => ClickHouseDbType.String,
                ColumnType.Text => ClickHouseDbType.String,
                _ => throw new InputArgumentException($"Parameter type {source.Type} is not supported")
            };

            if (source.Array)
            {
                clickHouseTarget.IsArray = true;
                clickHouseTarget.ArrayRank = 1;

                //An array parameter must have a specific element type
                var arrayValue = (object[])target.Value;
                target.Value = source.Type switch
                {
                    ColumnType.Boolean => arrayValue.Cast<bool>().ToArray(),
                    ColumnType.Integer => arrayValue.Cast<int>().ToArray(),
                    ColumnType.Long => arrayValue.Cast<long>().ToArray(),
                    ColumnType.Double => arrayValue.Cast<double>().ToArray(),
                    ColumnType.DateTime => arrayValue.Cast<DateTime>().ToArray(),
                    ColumnType.Guid => arrayValue.Cast<Guid>().ToArray(),
                    ColumnType.String => arrayValue.Cast<string>().ToArray(),
                    ColumnType.Text => arrayValue.Cast<string>().ToArray(),
                    _ => throw new InputArgumentException($"Parameter type {source.Type} is not supported")
                };
            }
        }
    }
}
