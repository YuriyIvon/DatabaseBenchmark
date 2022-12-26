using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core;

namespace DatabaseBenchmark.Commands.Options
{
    internal class InsertCommandOptions : QueryExecutionOptions
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

        [Option("Trace queries text and parameters")]
        public bool TraceQueries { get; set; } = false;

        [Option("Output report formatter type")]
        public string ReportFormatterType { get; set; } = "Text";

        [Option("Path to output report file")]
        public string ReportFilePath { get; set; }

        [Option("Report columns to be shown")]
        public string[] ReportColumns { get; set; }

        [Option("Report custom metric columns to be shown for each custom metric")]
        public string[] ReportCustomMetricColumns { get; set; }
    }
}
