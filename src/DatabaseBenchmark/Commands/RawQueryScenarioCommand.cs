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

            var scenarioSteps = CommandUtils.FilterByIndexes(queryScenario.Items, options.QueryScenarioStepIndexes);

            string parametersJson = !string.IsNullOrEmpty(options.QueryScenarioParametersFilePath)
                ? File.ReadAllText(options.QueryScenarioParametersFilePath)
                : null;

            try
            {
                foreach (var rawScenarioStep in scenarioSteps)
                {
                    var jsonOptionsProvider = new JsonOptionsProvider(rawScenarioStep.ToString(), parametersJson);
                    var scenarioStep = jsonOptionsProvider.GetOptions<RawQueryBenchmarkOptions>();

                    var databaseFactory = new DatabaseFactory(_environment, jsonOptionsProvider);
                    var database = databaseFactory.Create(scenarioStep.DatabaseType, scenarioStep.ConnectionString);
                    var query = new RawQuery
                    {
                        Text = File.ReadAllText(scenarioStep.QueryFilePath),
                        TableName = scenarioStep.TableName,
                    };

                    if (scenarioStep.QueryParametersFilePath != null)
                    {
                        query.Parameters = JsonUtils.DeserializeFile<RawQueryParameter[]>(scenarioStep.QueryParametersFilePath);
                    }

                    var executorFactory = database.CreateRawQueryExecutorFactory(query);

                    benchmark.Benchmark(executorFactory, scenarioStep);
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
