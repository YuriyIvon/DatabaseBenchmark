using DatabaseBenchmark.Common;
using DatabaseBenchmark.Databases.Sql.Interfaces;
using DatabaseBenchmark.Model;
using System.Data;

namespace DatabaseBenchmark.Databases.PostgreSql
{
    public class PostgreSqlJsonbParameterAdapter : PostgreSqlParameterAdapter
    {
        public override void Populate(SqlQueryParameter source, IDbDataParameter target)
        {
            if (source.Type == ColumnType.DateTime && source.Value != null)
            {
                var dateTimeValue = (DateTime)source.Value;
                source = new SqlQueryParameter(source.Prefix, source.Name, dateTimeValue.ToSortableString(), ColumnType.String, false);
            }

            base.Populate(source, target);
        }
    }
}
