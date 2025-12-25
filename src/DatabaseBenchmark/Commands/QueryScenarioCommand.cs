using DatabaseBenchmark.Commands.Interfaces;
using DatabaseBenchmark.Commands.Options;
using DatabaseBenchmark.Common;
using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Interfaces;
using DatabaseBenchmark.Databases;
using DatabaseBenchmark.Databases.Common.Interfaces;
using DatabaseBenchmark.DataSources;
using DatabaseBenchmark.DataSources.Interfaces;
using DatabaseBenchmark.Generators;
using DatabaseBenchmark.Generators.Interfaces;
using DatabaseBenchmark.Generators.Options;
using DatabaseBenchmark.Model;
using DatabaseBenchmark.Reporting;

namespace DatabaseBenchmark.Commands
{
    public class QueryScenarioCommand : ICommand
    {
        private readonly IOptionsProvider _optionsProvider;

        private IExecutionEnvironment _environment;

        public QueryScenarioCommand(IOptionsProvider optionsProvider)
        {
            _optionsProvider = optionsProvider;
        }

        public void Execute()
        {
            var options = _optionsProvider.GetOptions<QueryScenarioCommandOptions>();

            _environment = new ExecutionEnvironment(options.TraceQueries, options.TraceResults);
            var metricsCollector = new MetricsCollector();
            var resultsBuilder = new ResultsBuilder(options.ReportColumns, options.ReportCustomMetricColumns);
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
                using var currentDirectoryHolder = new CurrentDirectoryHolder();
                var queryScenarioFolder = Path.GetDirectoryName(Path.GetFullPath(options.QueryScenarioFilePath));
                Directory.SetCurrentDirectory(queryScenarioFolder);

                foreach (var rawScenarioStep in scenarioSteps)
                {
                    var jsonOptionsProvider = new JsonOptionsProvider(rawScenarioStep.ToString(), parametersJson);
                    var scenarioStep = jsonOptionsProvider.GetOptions<QueryBenchmarkOptions>();

                    var databaseFactory = new DatabaseFactory(_environment, jsonOptionsProvider);
                    var database = databaseFactory.Create(scenarioStep.DatabaseType, scenarioStep.ConnectionString);

                    var pluginRepository = !string.IsNullOrEmpty(options.PluginsFilePath)
                        ? new Plugins.PluginRepository(options.PluginsFilePath)
                        : null;

                    var dataSourceFactory = new DataSourceFactory(database, databaseFactory, _optionsProvider, pluginRepository);
                    var generatorFactory = new GeneratorFactory(dataSourceFactory, database, pluginRepository, null);

                    var table = JsonUtils.DeserializeFile<Table>(scenarioStep.TableFilePath);
                    var query = JsonUtils.DeserializeFile<Query>(scenarioStep.QueryFilePath, new GeneratorOptionsConverter());

                    if (!string.IsNullOrEmpty(scenarioStep.TableName))
                    {
                        table.Name = scenarioStep.TableName;
                    }

                    var executorFactory = database.CreateQueryExecutorFactory(table, query)
                        .Customize<IGeneratorFactory>(() => generatorFactory);

                    benchmark.Benchmark(executorFactory, scenarioStep);
                }

                ReportUtils.PrintReport(resultsBuilder, metricsCollector, options, _environment);
            }
            catch
            {
                metricsCollector.Abort();
                ReportUtils.PrintReport(resultsBuilder, metricsCollector, options, _environment);

                throw;
            }
        }
    }
}
