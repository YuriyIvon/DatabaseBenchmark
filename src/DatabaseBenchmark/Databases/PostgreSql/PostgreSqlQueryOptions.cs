using DatabaseBenchmark.Common;

namespace DatabaseBenchmark.Databases.PostgreSql
{
    [OptionPrefix("Postgres")]
    public class PostgreSqlQueryOptions
    {
        [Option("Use GIN-specific query operators on array columns")]
        public bool UseArrayGinOperators { get; set; } = false;
    }
}
