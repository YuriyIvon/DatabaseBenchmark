using DatabaseBenchmark.Common;

namespace DatabaseBenchmark.Commands.Options.Interfaces
{
    public interface IDataSourceOptions
    {
        [Option("Data source type", true)]
        string DataSourceType { get; set; }

        [Option("Path to a data file in case of a file-based data source or to a data source definition file otherwise", true)]
        string DataSourceFilePath { get; set; }

        [Option("Path to a JSON file describing the mapping between the data source and the table")]
        string MappingFilePath { get; set; }
    }
}
