using DatabaseBenchmark.Commands.Options.Interfaces;
using DatabaseBenchmark.Common;

namespace DatabaseBenchmark.Commands.Options
{
    public class QueryScenarioCommandOptions :
        IScenarioOptions,
        IReportOptions,
        IQueryTraceOptions,
        IResultTraceOptions
    {
        public string QueryScenarioFilePath { get; set; }

        public string QueryScenarioStepIndexes { get; set; }

        public string QueryScenarioParametersFilePath { get; set; }

        public string ReportFormatterType { get; set; } = "Text";

        public string ReportFilePath { get; set; }

        public string[] ReportColumns { get; set; }

        public string[] ReportCustomMetricColumns { get; set; }

        public bool TraceQueries { get; set; } = false;

        public bool TraceResults { get; set; } = false;
    }
}
