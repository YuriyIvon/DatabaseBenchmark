using DatabaseBenchmark.Common;

namespace DatabaseBenchmark.Databases.CosmosDb
{
    [OptionPrefix("CosmosDb")]
    public class CosmosDbQueryOptions
    {
        [Option("A maximum number of items to be fetched in one round-trip")]
        public int? BatchSize { get; set; }
    }
}
