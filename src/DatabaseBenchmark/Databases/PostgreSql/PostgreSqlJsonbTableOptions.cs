using DatabaseBenchmark.Common;

namespace DatabaseBenchmark.Databases.PostgreSql
{
    [OptionPrefix("PostgresJsonb")]
    public class PostgreSqlJsonbTableOptions
    {
        [Option("Create GIN index on the jsonb field")]
        public bool CreateGinIndex { get; set; } = true;
    }
}
