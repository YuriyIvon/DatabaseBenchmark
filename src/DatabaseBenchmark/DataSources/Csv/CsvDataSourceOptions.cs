using DatabaseBenchmark.Common;

namespace DatabaseBenchmark.DataSources.Csv
{
    [OptionPrefix("DataSource.Csv")]
    public class CsvDataSourceOptions
    {
        [Option("Column delimiter")]
        public string Delimiter { get; set; }

        [Option("Culture identifier used for parsing (the current system culture is used by default)")]
        public string Culture { get; set; }
    }
}
