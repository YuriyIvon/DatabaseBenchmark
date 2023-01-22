using DatabaseBenchmark.Common;

namespace DatabaseBenchmark.Commands.Options.Interfaces
{
    public interface IReportOptions
    {
        [Option("Output report formatter type")]
        string ReportFormatterType { get; set; }

        [Option("Path to output report file")]
        string ReportFilePath { get; set; }

        [Option("Report columns to be shown")]
        string[] ReportColumns { get; set; }

        [Option("Report custom metric columns to be shown for each custom metric")]
        string[] ReportCustomMetricColumns { get; set; }
    }
}
