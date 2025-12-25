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
            var resultsBuilder = new ResultsBuilder(options.ReportColumns, options.ReportCustomMetricColumns);
            var benchmark = new QueryBenchmark(_environment, metricsCollector);

            var databaseFactory = new DatabaseFactory(_environment, _optionsProvider);
            var database = databaseFactory.Create(options.DatabaseType, options.ConnectionString);

            var pluginRepository = !string.IsNullOrEmpty(options.PluginsFilePath)
                ? new Plugins.PluginRepository(options.PluginsFilePath)
                : null;

            var dataSourceFactory = new DataSourceFactory(database, databaseFactory, _optionsProvider, pluginRepository);
            var generatorFactory = new GeneratorFactory(dataSourceFactory, database, pluginRepository, null);

            var query = new RawQuery
            {
                Text = File.ReadAllText(options.QueryFilePath),
                TableName = options.TableName
            };

            if (options.QueryParametersFilePath != null)
            {
                query.Parameters = JsonUtils.DeserializeFile<RawQueryParameter[]>(options.QueryParametersFilePath, new GeneratorOptionsConverter());
            }

            var executorFactory = database.CreateRawQueryExecutorFactory(query)
                .Customize<IGeneratorFactory>(() => generatorFactory);

            benchmark.Benchmark(executorFactory, options);

            ReportUtils.PrintReport(resultsBuilder, metricsCollector, options, _environment);
        }
    }
}
