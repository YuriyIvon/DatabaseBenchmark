using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core;
using DatabaseBenchmark.Reporting;

namespace DatabaseBenchmark.Commands.Options
{
    internal class RawQueryCommandOptions : RawQueryBenchmarkOptions
    {
        [Option("Output report formatter type")]
        public string ReportFormatterType { get; set; } = "Text";

        [Option("Path to output report file")]
        public string ReportFilePath { get; set; }

        [Option("Trace queries text and parameters")]
        public bool TraceQueries { get; set; } = false;

        [Option("Trace query results")]
        public bool TraceResults { get; set; } = false;
    }
}
