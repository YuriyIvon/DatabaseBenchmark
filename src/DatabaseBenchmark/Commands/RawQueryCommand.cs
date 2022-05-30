using DatabaseBenchmark.Commands.Interfaces;
using DatabaseBenchmark.Commands.Options;
using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases;
using DatabaseBenchmark.Model;
using DatabaseBenchmark.Reporting;
using DatabaseBenchmark.Utils;

namespace DatabaseBenchmark.Commands
{
    public class RawQueryCommand : ICommand
    {
        private readonly IOptionsProvider _optionsProvider;

        private IExecutionEnvironment _environment;

        public RawQueryCommand(IOptionsProvider optionsProvider)
        {
            _optionsProvider = optionsProvider;
        }

        public void Execute()
        {
            var options = _optionsProvider.GetOptions<RawQueryCommandOptions>();

            _environment = new ExecutionEnvironment(options.TraceQueries, options.TraceResults);
            var metricsCollector = new MetricsCollector();
            var benchmark = new QueryBenchmark(_environment, metricsCollector);

            var databaseFactory = new DatabaseFactory(_environment, _optionsProvider);
            var database = databaseFactory.Create(options.DatabaseType, options.ConnectionString);
            var query = new RawQuery
            {
                Text = File.ReadAllText(options.QueryFilePath),
                TableName = options.TableName
            };

            if (options.QueryParametersFilePath != null)
            {
                query.Parameters = JsonUtils.DeserializeFile<RawQueryParameter[]>(options.QueryParametersFilePath);
            }

            var executorFactory = database.CreateRawQueryExecutorFactory(query);
            benchmark.Benchmark(executorFactory, options);

            Report(metricsCollector, options);
        }

        private void Report(MetricsCollector metricsCollector, RawQueryCommandOptions options)
        {
            var resultsBuilder = new ResultsBuilder();
            var reportFormatterFactory = new ReportFormatterFactory();
            var reportFormatter = reportFormatterFactory.Create(options.ReportFormatterType);

            var reporter = new BenchmarkReporter(resultsBuilder, reportFormatter, _environment, options.ReportFilePath);
            reporter.Report(metricsCollector);
        }
    }
}
