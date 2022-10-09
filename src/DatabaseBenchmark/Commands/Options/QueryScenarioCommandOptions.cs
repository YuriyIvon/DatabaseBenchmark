using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core;
using DatabaseBenchmark.Reporting;

namespace DatabaseBenchmark.Commands.Options
{
    public class QueryScenarioCommandOptions
    {
        [Option("Path to a JSON file describing the query scenario", true)]
        public string QueryScenarioFilePath { get; set; }

        [Option("Comma-separated list of scenario item indexes to run")]
        public string QueryScenarioStepIndexes { get; set; }

        [Option("Path to JSON file specifying scenario parameters")]
        public string QueryScenarioParametersFilePath { get; set; }

        [Option("Output report formatter type")]
        public string ReportFormatterType { get; set; } = "Text";

        [Option("Path to output report file")]
        public string ReportFilePath { get; set; }

        [Option("Report columns to be shown")]
        public string[] ReportColumns { get; set; }

        [Option("Report custom metric columns to be shown for each custom metric")]
        public string[] ReportCustomMetricColumns { get; set; }

        [Option("Trace queries text and parameters")]
        public bool TraceQueries { get; set; } = false;

        [Option("Trace query results")]
        public bool TraceResults { get; set; } = false;
    }
}
