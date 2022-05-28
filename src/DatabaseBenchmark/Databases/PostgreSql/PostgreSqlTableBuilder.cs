using DatabaseBenchmark.Databases.Sql;
using DatabaseBenchmark.Model;

namespace DatabaseBenchmark.Databases.PostgreSql
{
    public class PostgreSqlTableBuilder : SqlTableBuilder
    {
        protected override string BuildColumnType(Column column) => PostgreSqlDatabaseUtils.BuildColumnType(column);
    }
}
