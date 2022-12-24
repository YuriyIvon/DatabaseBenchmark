using DatabaseBenchmark.Databases.Sql;
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
                source = new SqlQueryParameter(source.Prefix, source.Name, dateTimeValue.ToString("s"), ColumnType.String);
            }

            base.Populate(source, target);
        }
    }
}
