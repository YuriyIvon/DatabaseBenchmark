using DatabaseBenchmark.Common;

namespace DatabaseBenchmark.DataSources.Csv
{
    [OptionPrefix("DataSource.Csv")]
    public class CsvDataSourceOptions
    {
        [Option("Column delimiter")]
        public string Delimiter { get; set; }
    }
}
