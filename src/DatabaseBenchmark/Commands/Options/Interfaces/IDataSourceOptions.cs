using DatabaseBenchmark.Common;

namespace DatabaseBenchmark.Commands.Options.Interfaces
{
    public interface IDataSourceOptions
    {
        [Option("Data source type", true)]
        string DataSourceType { get; set; }

        [Option("Path to a data file in case of a file-based data source or to a data source definition file otherwise", true)]
        string DataSourceFilePath { get; set; }

        [Option("Culture identifier used for parsing input string values if a string value is mapped to a non-string column. The current system culture is used by default.")]
        public string DataSourceCulture { get; set; }

        [Option("The maximum number of rows that the data source can return")]
        int DataSourceMaxRows { get; set; }

        [Option("Path to a JSON file describing the mapping between the data source and the table")]
        string MappingFilePath { get; set; }
    }
}
