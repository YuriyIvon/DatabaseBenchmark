using DatabaseBenchmark.Common;

namespace DatabaseBenchmark.Databases.PostgreSql
{
    [OptionPrefix("PostgresJsonb")]
    public class PostgreSqlJsonbQueryOptions
    {
        [Option("Use GIN-specific query operators such as @> where possible")]
        public bool UseGinOperators { get; set; } = true;
    }
}
