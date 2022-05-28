using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core;

namespace DatabaseBenchmark.Databases.ClickHouse
{
    [OptionPrefix("ClickHouse")]
    public class ClickHouseTableOptions
    {
        [Option("Table engine to be used")]
        public string Engine { get; set; } = "MergeTree()";

        [Option("Table sort order expression")]
        public string OrderBy { get; set; } = "tuple()";
    }
}
