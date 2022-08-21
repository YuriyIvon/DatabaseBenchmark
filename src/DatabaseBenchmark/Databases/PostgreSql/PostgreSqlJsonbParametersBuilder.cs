using DatabaseBenchmark.Databases.Sql;

namespace DatabaseBenchmark.Databases.PostgreSql
{
    public class PostgreSqlJsonbParametersBuilder : SqlParametersBuilder
    {
        protected override object PrepareValue(object value) =>
            value is DateTime dateTimeValue ? dateTimeValue.ToString("s") : value;
    }
}
