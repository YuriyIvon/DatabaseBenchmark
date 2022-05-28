using DatabaseBenchmark.Commands.Interfaces;
using DatabaseBenchmark.Commands.Options;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases;
using DatabaseBenchmark.Model;
using DatabaseBenchmark.Reporting;
using DatabaseBenchmark.Utils;
using System.Text.Json;

namespace DatabaseBenchmark.Commands
{
    public class RawQueryScenarioCommand : ICommand
    {
        private readonly IOptionsProvider _optionsProvider;

        private IExecutionEnvironment _environment;

        public RawQueryScenarioCommand(IOptionsProvider optionsProvider)
        {
            _optionsProvider = optionsProvider;
        }

        public void Execute()
        {
            var options = _optionsProvider.GetOptions<QueryScenarioCommandOptions>();

            _environment = new ExecutionEnvironment(options.TraceQueries, options.TraceResults);
            var metricsCollector = new MetricsCollector();
            var benchmark = new QueryBenchmark(_environment, metricsCollector);

            var queryScenario = JsonUtils.DeserializeFile<QueryScenario>(options.QueryScenarioFilePath);

            if (queryScenario.Items == null)
            {
                throw new InputArgumentException("Scenario items are not specified");
            }

            var scenarioItems = CommandUtils.FilterByIndexes(queryScenario.Items, options.QueryScenarioItemIndexes);

            string parametersJson = !string.IsNullOrEmpty(options.QueryScenarioParametersFilePath)
                ? File.ReadAllText(options.QueryScenarioParametersFilePath)
                : null;

            try
            {
                foreach (var rawScenarioItem in scenarioItems)
                {
                    var jsonOptionsProvider = new JsonOptionsProvider(rawScenarioItem.ToString(), parametersJson);
                    var scenarioItem = jsonOptionsProvider.GetOptions<RawQueryBenchmarkOptions>();

                    var databaseFactory = new DatabaseFactory(_environment, jsonOptionsProvider);
                    var database = databaseFactory.Create(scenarioItem.DatabaseType, scenarioItem.ConnectionString);
                    var query = JsonUtils.DeserializeFile<RawQuery>(scenarioItem.QueryFilePath);

                    var executorFactory = database.CreateRawQueryExecutorFactory(query);

                    benchmark.Benchmark(executorFactory, scenarioItem);
                }

                Report(metricsCollector, options);
            }
            catch
            {
                metricsCollector.Abort();
                Report(metricsCollector, options);

                throw;
            }
        }

        private void Report(MetricsCollector metricsCollector, QueryScenarioCommandOptions options)
        {
            var resultsBuilder = new ResultsBuilder();
            var reportFormatterFactory = new ReportFormatterFactory();
            var reportFormatter = reportFormatterFactory.Create(options.ReportFormatterType);

            var reporter = new BenchmarkReporter(resultsBuilder, reportFormatter, _environment, options.ReportFilePath);
            reporter.Report(metricsCollector);
        }
    }
}
