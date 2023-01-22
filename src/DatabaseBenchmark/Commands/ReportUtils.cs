using DatabaseBenchmark.Commands.Options.Interfaces;
using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Reporting;

namespace DatabaseBenchmark.Commands
{
    public static class ReportUtils
    {
        public static void PrintReport(
            ResultsBuilder resultsBuilder,
            MetricsCollector metricsCollector,
            IReportOptions options,
            IExecutionEnvironment environment)
        {
            var reportFormatterFactory = new ReportFormatterFactory();
            var reportFormatter = reportFormatterFactory.Create(options.ReportFormatterType);

            var reporter = new BenchmarkReporter(resultsBuilder, reportFormatter, environment, options.ReportFilePath);
            reporter.Report(metricsCollector);
        }
    }
}
