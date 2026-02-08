using DatabaseBenchmark.Common;

namespace DatabaseBenchmark.Databases.AzureSearch
{
    [OptionPrefix("AzureSearch")]
    public class AzureSearchInsertOptions
    {
        [Option("Base64-encode primary key values to satisfy Azure Search character constraints")]
        public bool Base64EncodePrimaryKey { get; set; } = false;
    }
}
