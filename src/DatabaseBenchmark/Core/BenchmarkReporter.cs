using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Interfaces.Reporting;
using DatabaseBenchmark.Reporting;
using DatabaseBenchmark.Reporting.Interfaces;

namespace DatabaseBenchmark.Core
{
    public class BenchmarkReporter
    {
        private readonly IResultsBuilder _resultsBuilder;
        private readonly IReportFormatter _reportFormatter;
        private readonly IExecutionEnvironment _environment;
        private readonly string _reportFilePath;

        public BenchmarkReporter(
            IResultsBuilder resultsBuilder,
            IReportFormatter reportFormatter, 
            IExecutionEnvironment environment,
            string reportFilePath)
        {
            _resultsBuilder = resultsBuilder;
            _reportFormatter = reportFormatter;
            _environment = environment;
            _reportFilePath = reportFilePath;
        }

        public void Report(MetricsCollector metricsCollector)
        {
            if (metricsCollector.Collections.Any())
            {
                var results = _resultsBuilder.Build(metricsCollector.Collections);

                using var outputStream = GetOutputStream();
                _reportFormatter.Print(outputStream, results);
            }
            else
            {
                _environment.WriteLine("No results have been collected");
            }
        }

        private Stream GetOutputStream() =>
            string.IsNullOrEmpty(_reportFilePath) ? Console.OpenStandardOutput() : File.Create(_reportFilePath);
    }
}
