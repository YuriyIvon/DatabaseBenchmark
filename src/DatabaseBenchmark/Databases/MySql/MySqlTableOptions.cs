using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core;

namespace DatabaseBenchmark.Databases.MySql
{
    [OptionPrefix("MySql")]
    public class MySqlTableOptions
    {
        [Option("Table engine to be used")]
        public string Engine { get; set; } = "InnoDB";
    }
}
