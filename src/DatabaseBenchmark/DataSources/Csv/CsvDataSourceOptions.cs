using DatabaseBenchmark.Common;

namespace DatabaseBenchmark.DataSources.Csv
{
    [OptionPrefix("DataSource.Csv")]
    public class CsvDataSourceOptions
    {
        [Option("Column delimiter")]
        public string Delimiter { get; set; }

        [Option("Specifies whether to treat blank values as null")]
        public bool TreatBlankAsNull { get; set; } = true;
    }
}
