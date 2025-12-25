using DatabaseBenchmark.Commands.Options.Interfaces;
using DatabaseBenchmark.Common;

namespace DatabaseBenchmark.Commands.Options
{
    public class RawQueryCommandOptions :
        RawQueryBenchmarkOptions,
        IReportOptions,
        IQueryTraceOptions,
        IResultTraceOptions,
        IPluginOptions
    {
        public string ReportFormatterType { get; set; } = "Text";

        public string ReportFilePath { get; set; }

        public string[] ReportColumns { get; set; }

        public string[] ReportCustomMetricColumns { get; set; }

        public bool TraceQueries { get; set; } = false;

        public bool TraceResults { get; set; } = false;

        public string PluginsFilePath { get; set; }
    }
}
