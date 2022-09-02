using DatabaseBenchmark.Common;
using DatabaseBenchmark.Databases;
using DatabaseBenchmark.DataSources;

namespace DatabaseBenchmark.Commands.Options
{
    public class ImportCommandOptions
    {
        [Option("Database type", true)]
        public string DatabaseType { get; set; }

        [Option("Connection string", true)]
        public string ConnectionString { get; set; }

        [Option("Path to a JSON file describing the table structure", true)]
        public string TableFilePath { get; set; }

        [Option("Target physical table name")]
        public string TableName { get; set; }

        [Option("Data source type", true)]
        public string DataSourceType { get; set; }

        [Option("Path to a data file in case of a file-based data source or to a data source definition file otherwise", true)]
        public string DataSourceFilePath { get; set; }

        [Option("Path to a JSON file describing the mapping between the data source and the table")]
        public string MappingFilePath { get; set; }

        [Option("Number of records in a batch inserted into the database during import (each database type has its own default)")]
        public int ImportBatchSize { get; set; } = 0;

        [Option("Trace queries text and parameters")]
        public bool TraceQueries { get; set; } = false;
    }
}
